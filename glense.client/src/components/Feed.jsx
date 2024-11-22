import { useState } from "react";
import { Box, Stack } from "@mui/material";
import { Sidebar } from "./";

import "../css/Feed.css";

function Feed() {
    const [selectedCategory, setSelectedCategory] = useState("New Videos");

    return (
        <Stack className="feed-container">
            <Box className="feed-sidebar">
                <Sidebar
                    selectedCategory={selectedCategory}
                    setSelectedCategory={setSelectedCategory}
                />
            </Box>

            <Box className="feed-content" />
            </Stack>
    );
}

export default Feed;