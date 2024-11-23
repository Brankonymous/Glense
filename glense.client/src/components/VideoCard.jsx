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

function VideoCard({
    video: {
        id: { videoId },
    },
}) {
    return (
        <Card className="video-card">
            <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                <CardMedia
                    image={demoThumbnailUrl}
                    className="video-card-media"
                />
            </Link>

            <CardContent className="video-card-content">
                <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                    <Typography variant="subtitle1" className="video-card-title">
                        {demoVideoTitle.slice(0, 80)
                            + (demoVideoTitle.length > 80 ? "..." : "")
                        }
                    </Typography>
                </Link>
                <Link to={demoVideoUrl}>
                    <Typography variant="subtitle2" className="video-card-channel">
                        {demoChannelTitle}
                        <CheckCircle className="video-card-icon" />
                    </Typography>
                </Link>
            </CardContent>
        </Card>
    );
}

export default VideoCard;
