import { Stack, Box } from "@mui/material";
import { VideoCard } from "./";
import CardSkeleton from "./CardSkeleton";

function Videos({ videos }) {
    if (!videos?.length) return <CardSkeleton />;

    console.log("VIDEO ID " + videos);
    return (
        <Stack
            direction={"row"}
            flexWrap='wrap'
            justifyContent='center'
            alignItems='center'
            gap={2}
        >
            {videos.map((item, index) => (
                <Box key={index}>
                    {item.id.videoId && <VideoCard video={item} />}
                </Box>
            ))}
        </Stack>
    );
}

export default Videos;