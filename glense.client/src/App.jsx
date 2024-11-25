import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Box } from "@mui/material";

import {
    Navbar,
    Feed,
    VideoStream,
    ChannelDetail
} from "./components";
import Chat from "./components/Chat/Chat";

const App = () => {
    return (
        <Router>
            <div className="App">
                <Box>
                    <Navbar />
                    <Routes>
                        <Route path='/' element={<Feed />} />

                        <Route path='/video/:id' element={<VideoStream />} />
                        <Route path='/channel/:id' element={<ChannelDetail />} />
                        <Route path='/chat/:id' element={<Chat />} />
                    </Routes>
                </Box>
            </div>
        </Router>
    );
};

export default App;