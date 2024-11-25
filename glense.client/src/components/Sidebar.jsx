import { Stack } from "@mui/material";
import { categories } from "../utils/constants";

import "../css/Sidebar.css";

function Sidebar({ setSelectedCategory }) {
    return (
        <Stack className='sidebar-stack'>
            {categories.map((category) => (
                <button
                    key={category.name}
                    className='category-btn'
                    onClick={() => setSelectedCategory(category.name)}
                >
                    <span>{category.icon}</span>
                    <span>{category.name}</span>
                </button>
            ))}
        </Stack>
    );
}

export default Sidebar;