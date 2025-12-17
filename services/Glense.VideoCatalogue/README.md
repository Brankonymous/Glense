# Glense.VideoCatalogue

Microservice that manages video metadata, playlists, subscriptions and serves video files.

## Overview
- Implements a .NET 8 Web API using EF Core and a pluggable `IVideoStorage` for file storage.
- Local file storage is provided by `LocalFileVideoStorage` (configured by `VideoStorage:BasePath`).

## Prerequisites
- .NET 8 SDK
- (Optional) PostgreSQL for production; `Npgsql.EntityFrameworkCore.PostgreSQL` is included.

## Local development
Restore and run from repository root:

```powershell
dotnet restore services\Glense.VideoCatalogue
dotnet run --project services\Glense.VideoCatalogue
```

The service exposes Swagger UI at `/swagger` and a health endpoint at `/health`.

## Configuration
Add a connection string (example) to `appsettings.Development.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "VideoCatalogue": "Host=localhost;Database=video_catalogue;Username=postgres;Password=postgres"
  }
}
```

Video storage settings (in `appsettings.json`):

```json
"VideoStorage": {
  "BasePath": "Videos",
  "RequestBufferSize": 81920
}
```

## Migrations
Install `dotnet-ef` and create / apply migrations from the repo root:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.12
dotnet restore services\Glense.VideoCatalogue
dotnet ef migrations add InitialCreate --project services\Glense.VideoCatalogue --startup-project services\Glense.VideoCatalogue --output-dir Migrations
dotnet ef database update --project services\Glense.VideoCatalogue --startup-project services\Glense.VideoCatalogue
```

## Docker
Build and run the service with Docker:

```bash
docker build -t glense-videocatalogue:latest -f services/Glense.VideoCatalogue/Dockerfile .
docker run -p 5102:80 -e "ConnectionStrings__VideoCatalogue=Host=host.docker.internal;Database=video_catalogue;Username=postgres;Password=postgres" glense-videocatalogue:latest
```

## API examples
Upload a video (multipart/form-data):

```bash
curl -X POST "http://localhost:5102/api/videos/upload" \
  -F "file=@/path/to/video.mp4" \
  -F "uploaderId=1" \
  -F "title=My Video"
```

Stream a video (supports Range header):

```bash
curl -H "Range: bytes=0-" http://localhost:5102/api/videos/<VIDEO_ID>/stream --output part.mp4
```

## Notes & next steps
- Consider securing upload endpoints with authentication and deriving `uploaderId` from user claims.
- Add unit and integration tests under `services/Glense.VideoCatalogue/Tests`.
- For production, replace `LocalFileVideoStorage` with S3/Azure Blob implementation and configure CDN.
