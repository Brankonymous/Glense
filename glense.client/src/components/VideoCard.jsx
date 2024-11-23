/* eslint-disable react/prop-types */
import { Link } from "react-router-dom";
import { Typography, Card, CardContent, CardMedia } from "@mui/material";
import { CheckCircle } from "@mui/icons-material";

import {
    demoChannelTitle,
    demoVideoTitle,
    demoVideoUrl,
} from "../utils/constants";

function VideoCard({
    video: {
        id: { videoId }
    },
}) {
    console.log("VIDEO ID " + videoId);
    return (
        <Card
            sx={{
                width: { xs: "100%", sm: "358px", md: "320px" },
                boxShadow: "none",
                borderRadius: 0,
            }}
        >
            <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                <CardMedia
                    sx={{
                        width: "100%",
                        height: 150,
                    }}
                />
            </Link>

            <CardContent sx={{ backgroundColor: "hsl(0, 0%, 7%)", height: "106px" }}>
                <Link to={videoId ? `/video/${videoId}` : demoVideoUrl}>
                    <Typography variant='subtitle1' fontWeight='bold' color='#f1f1f1'>
                        {demoVideoTitle.slice(0, 60)}
                    </Typography>
                </Link>
                <Link
                    to={
                        demoVideoUrl
                    }
                >
                    <Typography variant='subtitle2' fontWeight='bold' color='gray'>
                        {demoChannelTitle}
                        <CheckCircle sx={{ fontSize: 12, color: "gray", ml: "5px" }} />
                    </Typography>
                </Link>
            </CardContent>
        </Card>
    );
}

export default VideoCard;