import { Stack, Typography, Button  } from "@mui/material";
import { Link } from "react-router-dom";
import logo from "../assets/logo_transparent.png";
import SearchBar from "../components/SearchBar";
import { useState } from "react";

import "../css/Navbar.css";
import { SignInPopup } from ".";

function Navbar() {
    const [open, setOpen] = useState(false);
    const channelId = "mkbhd";
    const shouldSignIn = false;

    return (
        <Stack>
            <div className="Navbar">
                <div className="navbar-container">
                    <div className="logo-container">
                        <Link to="/">
                            <img className="logo" src={logo} alt="Glense logo" />
                            <span className="logo-text">Glense</span>
                        </Link>
                    </div>

                    <div className="searchbar-container">
                        <SearchBar />
                    </div>

                    <div className="auth-container">
                        {!shouldSignIn ? (
                            <Stack className="navbar-option-stack">
                                <Link to={`/chat/${channelId}`}>
                                    <Typography className="navbar-option">
                                        Chat
                                    </Typography>
                                </Link>
                                <Link to="/donations">
                                    <Typography className="navbar-option">
                                        Donations
                                    </Typography>
                                </Link>
                                <Link to={`/channel/${channelId}`}>
                                    <Typography className="navbar-option">
                                        Profile
                                    </Typography>
                                </Link>
                            </Stack>
                        ): (
                            <div>
                                <Button className="navbar-button" onClick={() => setOpen(true)}>
                                    <Typography className="navbar-option">
                                        Sign-in
                                    </Typography>
                                </Button>
                                <SignInPopup open={open} onClose={() => setOpen(false)} />
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </Stack>
    );
}

export default Navbar;