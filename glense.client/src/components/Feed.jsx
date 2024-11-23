import { useState, useEffect } from "react";
import { Box, Stack } from "@mui/material";
import { Sidebar, Videos } from "./";

import "../css/Feed.css";

function Feed() {
    const [selectedCategory, setSelectedCategory] = useState("Category changed");

    const videos = [];
    for (let i = 0; i < 100; i++) {
        videos.push({
            id: { videoId: "GDa8kZLNhJ4" },
            title: "Video " + i,
            url: "https://youtu.be/haDjmBT9tu4?si=E6PgeLsJ3Z-zKPAh"
        });
    }

    videos.forEach((item, index) => {
        console.log(item, index);  
    });

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