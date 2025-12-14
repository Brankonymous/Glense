#!/usr/bin/env pwsh
# Simple PowerShell test script for register/login and calling chat endpoints
# Update $accountBase and $chatBase to the correct ports shown when you run each service

$accountBase = $env:ACCOUNT_URLS -split ';' | Select-Object -First 1
if (-not $accountBase) { $accountBase = 'http://localhost:5000' }
$chatBase = $env:CHAT_URLS -split ';' | Select-Object -First 1
if (-not $chatBase) { $chatBase = 'http://localhost:5002' }

Write-Host "Registering test user..."
$reg = Invoke-RestMethod -Method Post -Uri "$accountBase/api/auth/register" -ContentType 'application/json' -Body (@{
    username='alice'
    email='alice@example.com'
    password='Password123!'
    confirmPassword='Password123!'
} | ConvertTo-Json)
Write-Host "Register response:"; $reg | ConvertTo-Json -Depth 5

Write-Host "Logging in..."
$login = Invoke-RestMethod -Method Post -Uri "$accountBase/api/auth/login" -ContentType 'application/json' -Body (@{
    usernameOrEmail='alice'
    password='Password123!'
} | ConvertTo-Json)
Write-Host "Login response:"; $login | ConvertTo-Json -Depth 5

$token = $login.token
Write-Host "Token: $($token.Substring(0,40))..."

# Call Chat GET /api/chats
Write-Host "Calling GET /api/chats"
$headers = @{ Authorization = "Bearer $token" }
$chats = Invoke-RestMethod -Method Get -Uri "$chatBase/api/chats" -Headers $headers
$chats | ConvertTo-Json -Depth 5

# Note: adjust endpoints and payloads depending on the ChatService API shape
Write-Host "Done. If you want to test SignalR, open the chat service URL + /signalr-test.html in your browser, paste the token into the JWT field and click Join/Send."
