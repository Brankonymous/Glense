import { Stack, Typography } from "@mui/material";
import { Link, Route } from "react-router-dom";
import logo from "../assets/logo_transparent.png";
import SearchBar from "../components/SearchBar";

import "../css/Navbar.css";

function Navbar() {
    const channelId = "mkbhd";
    const shouldSignIn = true;

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
                        {shouldSignIn ? (
                            <Stack className="navbar-option-stack">
                                <Link to={`/chat/${channelId}`}>
                                    <Typography className="navbar-option">
                                        Chat
                                    </Typography>
                                </Link>
                                <Link to={`/channel/${channelId}`}>
                                    <Typography className="navbar-option">
                                        Profile
                                    </Typography>
                                </Link>
                            </Stack>
                        ): (
                            <Link to={`/sign-on`}>
                                <Typography className="navbar-option">
                                    Sign-in
                                </Typography>
                            </Link>
                        )}
                    </div>
                </div>
            </div>
        </Stack>
    );
}

export default Navbar;