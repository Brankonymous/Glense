import { useState } from "react";
import { Box, Stack } from "@mui/material";
import { Sidebar, Videos } from "./";

import "../css/Feed.css";
import { videos } from "../utils/constants";

function Feed() {
    const [selectedCategory, setSelectedCategory] = useState("Category changed");

    return (
        <Stack className="feed-container">
            <Box className="feed-sidebar">
                <Sidebar
                    selectedCategory={selectedCategory}
                    setSelectedCategory={setSelectedCategory}
                />
            </Box>

            <Box className="feed-content" >
                <Videos videos={videos} />
            </Box>
        </Stack>
    );
}

export default Feed;