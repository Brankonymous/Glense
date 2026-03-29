import { useState, useEffect, useMemo } from "react";
import { Box, Stack } from "@mui/material";
import { Sidebar, Videos } from "./";

import "../css/Feed.css";
import { getVideos } from "../utils/videoApi";

function Feed() {
    const [selectedCategory, setSelectedCategory] = useState("New Videos");
    const [items, setItems] = useState([]);

    useEffect(() => {
        let mounted = true;
        getVideos()
            .then(data => { if (mounted && Array.isArray(data)) setItems(data); })
            .catch(() => {});
        return () => { mounted = false; };
    }, []);

    const filtered = useMemo(() => {
        if (!selectedCategory || selectedCategory === "New Videos") return items;
        return items.filter(v => v.category === selectedCategory);
    }, [items, selectedCategory]);

    return (
        <Stack className="feed-container">
            <Box className="feed-sidebar">
                <Sidebar
                    selectedCategory={selectedCategory}
                    setSelectedCategory={setSelectedCategory}
                />
            </Box>

            <Box className="feed-content" >
                <Videos videos={filtered} />
            </Box>
        </Stack>
    );
}

export default Feed;
