import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Box } from "@mui/material";

import {
    Navbar,
    Feed,
    VideoStream
} from "./components";

const App = () => {
    return (
        <Router>
            <div className="App">
                <Box>
                    <Navbar />
                    <Routes>
                        <Route path='/' element={<Feed />} />

                        <Route path='/video/:id' element={<VideoStream />} />
                    </Routes>
                </Box>
            </div>
        </Router>
    );
};

export default App;