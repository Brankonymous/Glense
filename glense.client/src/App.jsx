import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Box } from "@mui/material";

import {
    Navbar,
    Feed,
    VideoStream,
    ChannelDetail,
    Donations
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
                        <Route path='/donations' element={<Donations />} />
                    </Routes>
                </Box>
            </div>
        </Router>
    );
};

export default App;