import { Stack, Box } from "@mui/material";
import { VideoCard } from "./";

import "../css/Videos.css"; // Importing the CSS file

function Videos({ videos, direction }) {
    return (
        <Box className="videos-container">
            <Stack
                className="videos-stack"
                direction={direction || "row"}
            >
                {videos.map((item, index) => (
                    <Box key={index} className="video-box">
                        {item.id.videoId && <VideoCard video={item} />}
                    </Box>
                ))}
            </Stack>
        </Box>
    );
}

export default Videos;
