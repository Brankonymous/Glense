/* eslint-disable react/prop-types */
import { Link } from "react-router-dom";
import { Typography, Card, CardContent, CardMedia } from "@mui/material";
import { CheckCircle } from "@mui/icons-material";

import {
    demoChannelTitle,
    demoVideoTitle,
    demoVideoUrl,
    demoThumbnailUrl,
} from "../utils/constants";

import "../css/VideoCard.css";

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5050';

function VideoCard({ video }) {
    // Support both YouTube-like shape ({ id: { videoId } }) and catalogue shape ({ id: "guid", thumbnailUrl, title })
    const nestedId = video?.id?.videoId;
    const simpleId = video?.id && typeof video.id === 'string' ? video.id : null;
    const videoId = nestedId || simpleId || video?.videoId;
    const rawThumb = video?.thumbnailUrl;
    const thumbnail = rawThumb
        ? (rawThumb.startsWith('http') ? rawThumb : `${API_BASE}${rawThumb}`)
        : demoThumbnailUrl;
    const title = video?.title || demoVideoTitle;

    return (
        <Card className="video-card">
            <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                <CardMedia
                    image={thumbnail}
                    className="video-card-media"
                />
            </Link>

            <CardContent className="video-card-content">
                <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                    <Typography variant="subtitle1" className="video-card-title">
                        {title.slice(0, 80) + (title.length > 80 ? "..." : "")}
                    </Typography>
                </Link>
                <Link to={video?.uploaderId ? `/channel/${video.uploaderId}` : demoVideoUrl}>
                    <Typography variant="subtitle2" className="video-card-channel">
                        {video?.uploaderUsername || demoChannelTitle}
                        <CheckCircle className="video-card-icon" />
                    </Typography>
                </Link>
            </CardContent>
        </Card>
    );
}

export default VideoCard;
