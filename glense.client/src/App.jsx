import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Box } from "@mui/material";

import {
    Navbar,
    Feed
} from "./components";

const App = () => {
    return (
        <Router>
            <div className="App">
                <Box>
                    <Navbar />
                    <Routes>
                        <Route path='/' element={<Feed />} />
                    </Routes>
                </Box>
            </div>
        </Router>
    );
};

export default App;