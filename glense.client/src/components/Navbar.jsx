import { Stack, Typography, Button, IconButton, Menu, MenuItem } from "@mui/material";
import { Link } from "react-router-dom";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import logo from "../assets/logo_transparent.png";
import SearchBar from "../components/SearchBar";
import { useState } from "react";
import { useAuth } from "../context/AuthContext";

import "../css/Navbar.css";
import { SignInPopup } from ".";

function Navbar() {
    const [open, setOpen] = useState(false);
    const [anchorEl, setAnchorEl] = useState(null);
    const { isAuthenticated, user, logout } = useAuth();
    const channelId = user?.username || "mkbhd";

    const handleMenuOpen = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    const handleLogout = () => {
        logout();
        handleMenuClose();
    };

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
                        {isAuthenticated ? (
                            <>
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
                                    <IconButton onClick={handleMenuOpen} sx={{ color: 'white' }}>
                                        <AccountCircleIcon />
                                    </IconButton>
                                </Stack>
                                <Menu
                                    anchorEl={anchorEl}
                                    open={Boolean(anchorEl)}
                                    onClose={handleMenuClose}
                                >
                                    <MenuItem disabled>
                                        <Typography variant="body2" color="textSecondary">
                                            {user?.username}
                                        </Typography>
                                    </MenuItem>
                                    <MenuItem onClick={handleMenuClose}>
                                        <Link to={`/channel/${channelId}`} style={{ textDecoration: 'none', color: 'inherit' }}>
                                            Profile
                                        </Link>
                                    </MenuItem>
                                    <MenuItem onClick={handleLogout}>Logout</MenuItem>
                                </Menu>
                            </>
                        ) : (
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
