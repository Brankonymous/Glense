import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Paper, IconButton } from "@mui/material";
import { Search } from "@mui/icons-material";

import "../css/Searchbar.css";

function SearchBar() {
    const [searchTerm, setSearchTerm] = useState("");

    const navigate = useNavigate();

    const handleSubmit = (e) => {
        e.preventDefault();
        navigate(`/search/${searchTerm}`);
        setSearchTerm("");
    };

    return (
        <Paper
            component='form'
            onSubmit={handleSubmit}
            className='search-bar-container'
        >
            <input
                type='text'
                className='search-bar'
                value={searchTerm}
                placeholder='Search..'
                onChange={(e) => setSearchTerm(e.target.value)}
            />

            <IconButton>
                <Search className='search-icon'/>
            </IconButton>
        </Paper>
    );
}

export default SearchBar;