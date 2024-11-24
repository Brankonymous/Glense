import { Stack, Box } from "@mui/material";
import { VideoCard } from "./";

function Videos({ videos, direction }) {
    return (
        <Box marginTop='3rem'
          height='90%'
          overflow={'auto'}
        >
            <Stack
                direction={direction || "row"}
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
        </Box>
    );
}

export default Videos;