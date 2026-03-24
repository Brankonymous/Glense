import { useState, useEffect } from "react";
import { Box, Stack } from "@mui/material";
import { Sidebar, Videos } from "./";

import "../css/Feed.css";
import { getVideos } from "../utils/videoApi";

// Fetches videos from VideoCatalogue service and shows them in feed.

function Feed() {
    const [selectedCategory, setSelectedCategory] = useState("Category changed");
    const [items, setItems] = useState([]);

    useEffect(() => {
        let mounted = true;
        getVideos()
            .then(data => { if (mounted && Array.isArray(data)) setItems(data); })
            .catch(() => {});
        return () => { mounted = false; };
    }, []);

    return (
        <Stack className="feed-container">
            <Box className="feed-sidebar">
                <Sidebar
                    selectedCategory={selectedCategory}
                    setSelectedCategory={setSelectedCategory}
                />
            </Box>

            <Box className="feed-content" >
                <Videos videos={items} />
            </Box>
        </Stack>
    );
}

export default Feed;