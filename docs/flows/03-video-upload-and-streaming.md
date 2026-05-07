# Flow: Video upload & streaming

## Upload

```mermaid
sequenceDiagram
    participant FE as Frontend (Upload.jsx)
    participant GW as Gateway (5050)
    participant VC as Video Catalogue
    participant FS as Disk (VideoStorage:BasePath)
    participant DB as Postgres (video)

    FE->>GW: POST /api/videos/upload  (multipart, ≤500MB)
    GW->>VC: forward (cluster timeout 5min)
    VC->>VC: AuthN — JWT required
    VC->>FS: write file to {BasePath}/{guid}.{ext}
    VC->>DB: INSERT video {id, title, description, videoUrl, thumbnailUrl, uploaderId, ...}
    VC-->>FE: 201 Created { UploadResponseDTO }
```

## Stream

```mermaid
sequenceDiagram
    participant Player as <video> element
    participant GW as Gateway
    participant VC as Video Catalogue
    participant FS as Disk

    Player->>GW: GET /api/videos/{id}/stream  Range: bytes=0-
    GW->>VC: forward
    VC->>DB: lookup video.videoUrl
    VC->>FS: PhysicalFile(path, contentType, enableRangeProcessing: true)
    VC-->>Player: 206 Partial Content (Accept-Ranges: bytes)
```

## Code refs

- Upload + stream: [services/Glense.VideoCatalogue/Controllers/VideosController.cs](../../services/Glense.VideoCatalogue/Controllers/VideosController.cs) (`Upload`, `Stream`, `Thumbnail`)
- Storage abstraction: `IVideoStorage` / `LocalFileVideoStorage` in [services/Glense.VideoCatalogue/Services/](../../services/Glense.VideoCatalogue/Services)
- Frontend: [glense.client/src/components/Upload.jsx](../../glense.client/src/components/Upload.jsx), [VideoStream.jsx](../../glense.client/src/components/VideoStream.jsx)
- 500 MB Kestrel cap: [services/Glense.VideoCatalogue/Program.cs](../../services/Glense.VideoCatalogue/Program.cs)

## Failure modes

| Failure | Behavior |
|---------|----------|
| File > 500 MB | Kestrel rejects with `413 Payload Too Large` before reaching the controller |
| Disk write fails | Service returns `500`; the half-written file is left for cleanup |
| Range request not supported by client | Falls back to `200` full body |
| Account gRPC down at list-time | Video list still returned; `uploaderUsername` is `null` |

## Related APIs

- [`POST /api/videos/upload`](../api/video-catalogue.md)
- [`GET /api/videos/{id}/stream`](../api/video-catalogue.md)
- [`PATCH /api/videos/{id}/view`](../api/video-catalogue.md)
