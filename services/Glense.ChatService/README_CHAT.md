# Glense.ChatService - SignalR test and quick run

This project now exposes a small SignalR hub for manual testing and a static test page.

Run the service (from repository root):

```powershell
dotnet run --project .\services\Glense.ChatService\Glense.ChatService.csproj
```

Open the SignalR test page in your browser (service must be running):

http://localhost:<chat-port>/signalr-test.html

Paste a valid JWT into the JWT field (obtain from AccountService /api/auth/login). Then click Join, type a message, click Send.

If you want an automated quick test, run the repository-level PowerShell script:

```powershell
.\scripts\test_chat.ps1
```

Adjust `$accountBase` and `$chatBase` variables at the top of that script if your services run on different ports.
