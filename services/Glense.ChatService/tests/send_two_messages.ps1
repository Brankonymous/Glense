#!/usr/bin/env pwsh
# Create two users, create a chat, send two messages (A->B and B->A), then list messages

$accountBase = $env:ACCOUNT_URLS -split ';' | Select-Object -First 1
if (-not $accountBase) { $accountBase = 'http://localhost:5000' }
$chatBase = $env:CHAT_URLS -split ';' | Select-Object -First 1
if (-not $chatBase) { $chatBase = 'http://localhost:5002' }

Write-Host "Using AccountService: $accountBase"
Write-Host "Using ChatService: $chatBase"

function Ensure-Login($username, $email, $password) {
    Write-Host "Ensuring user $username"
    try {
        $reg = Invoke-RestMethod -Method Post -Uri "$accountBase/api/auth/register" -ContentType 'application/json' -Body (@{
            username = $username
            email = $email
            password = $password
            confirmPassword = $password
        } | ConvertTo-Json)
        Write-Host "Registered $username"
    } catch {
        Write-Host ("Register skipped/failed for {0}: {1}" -f $username, $_.Exception.Message)
    }

    try {
        $login = Invoke-RestMethod -Method Post -Uri "$accountBase/api/auth/login" -ContentType 'application/json' -Body (@{
            usernameOrEmail = $username
            password = $password
        } | ConvertTo-Json)
        Write-Host "Logged in $username"
        return $login.token
    } catch {
        Write-Error ("Login failed for {0}: {1}" -f $username, $_.Exception.Message)
        return $null
    }
}

function Invoke-Safe($method, $uri, $headers=$null, $body=$null) {
    try {
        if ($body -ne $null) {
            return Invoke-RestMethod -Method $method -Uri $uri -Headers $headers -ContentType 'application/json' -Body ($body | ConvertTo-Json)
        } else {
            return Invoke-RestMethod -Method $method -Uri $uri -Headers $headers
        }
    } catch {
        Write-Host "Request to $uri failed: $($_.Exception.Message)"
        if ($_.Exception.Response -ne $null) {
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $sr = New-Object System.IO.StreamReader($stream)
                $text = $sr.ReadToEnd()
                Write-Host "Response body:`n$text"
            } catch {
                Write-Host "Failed to read response body: $($_.Exception.Message)"
            }
        }
        return $null
    }
}

$pw = 'Password123!'
$tokenA = Ensure-Login 'userA_test' 'userA@example.com' $pw
Start-Sleep -Milliseconds 200
$tokenB = Ensure-Login 'userB_test' 'userB@example.com' $pw

if (-not $tokenA -or -not $tokenB) { Write-Error 'Failed to obtain both tokens. Aborting.'; exit 1 }

Write-Host "Token A: $($tokenA.Substring(0,30))..."
Write-Host "Token B: $($tokenB.Substring(0,30))..."

# Create a chat using A's token
$hdrA = @{ Authorization = "Bearer $tokenA" }
$topic = 'Chat A-B'
Write-Host 'Creating chat...'
$chat = Invoke-Safe -method 'Post' -uri "$chatBase/api/chats" -headers $hdrA -body @{ topic = $topic }
$chatId = $chat.id
Write-Host "Created chat: $chatId"

# Send message A -> B
Write-Host 'Sending message A -> B'
$msg1 = Invoke-RestMethod -Method Post -Uri "$chatBase/api/chats/$chatId/messages" -Headers $hdrA -ContentType 'application/json' -Body (@{ sender = 'user'; content = 'Hi B, this is A.' } | ConvertTo-Json)
Write-Host "Message created: $($msg1.id)"

# Send message B -> A
$hdrB = @{ Authorization = "Bearer $tokenB" }
Write-Host 'Sending message B -> A'
$msg2 = Invoke-RestMethod -Method Post -Uri "$chatBase/api/chats/$chatId/messages" -Headers $hdrB -ContentType 'application/json' -Body (@{ sender = 'user'; content = 'Hello A, B here.' } | ConvertTo-Json)
Write-Host "Message created: $($msg2.id)"

# List messages
Write-Host 'Fetching messages for chat'
$msgs = Invoke-RestMethod -Method Get -Uri "$chatBase/api/chats/$chatId/messages" -Headers $hdrA
Write-Host 'Messages JSON:'; $msgs | ConvertTo-Json -Depth 6

Write-Host 'Done.'
