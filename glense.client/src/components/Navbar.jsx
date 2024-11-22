import { Stack } from "@mui/material";
import { Link } from "react-router-dom";
import logo from "../assets/logo_transparent.png";
import SearchBar from "../components/SearchBar";

import "../css/Navbar.css";

function Navbar() {
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
                </div>
            </div>
        </Stack>
    );
}

export default Navbar;