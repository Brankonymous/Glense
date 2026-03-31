import { useState, useEffect, useMemo } from "react";
import { useParams, Link } from "react-router-dom";
import { Box, Typography, Stack, CircularProgress, Avatar } from "@mui/material";
import { CheckCircle } from "@mui/icons-material";

import { Videos } from "./";
import { searchVideos } from "../utils/videoApi";
import { profileService } from "../services/profileService";
import { categories } from "../utils/constants";

import "../css/SearchResults.css";

const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
function stringToColor(str) {
  let h = 0;
  for (let i = 0; i < (str || '').length; i++) h = str.charCodeAt(i) + ((h << 5) - h);
  return colors[Math.abs(h) % colors.length];
}

function SearchResults() {
  const { searchTerm } = useParams();
  const [videos, setVideos] = useState([]);
  const [channels, setChannels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState(null);

  const filteredVideos = useMemo(() => {
    if (!selectedCategory) return videos;
    return videos.filter(v => v.category && v.category.toLowerCase() === selectedCategory.toLowerCase());
  }, [videos, selectedCategory]);

  useEffect(() => {
    if (!searchTerm) return;

    let mounted = true;
    setLoading(true);
    setVideos([]);
    setChannels([]);
    setSelectedCategory(null);

    Promise.all([
      searchVideos(searchTerm).catch(() => []),
      profileService.searchUsers(searchTerm).catch(() => []),
    ]).then(([videoData, userData]) => {
      if (!mounted) return;
      if (Array.isArray(videoData)) setVideos(videoData);
      if (Array.isArray(userData)) setChannels(userData);
      setLoading(false);
    });

    return () => { mounted = false; };
  }, [searchTerm]);

  if (loading) {
    return (
      <Box className="search-results-container">
        <Box className="search-results-loading">
          <CircularProgress sx={{ color: 'var(--color-text-subtle)' }} />
        </Box>
      </Box>
    );
  }

  const noResults = videos.length === 0 && channels.length === 0;

  return (
    <Box className="search-results-container">
      <Typography variant="h5" className="search-results-heading">
        Search results for &quot;{searchTerm}&quot;
      </Typography>

      {noResults && (
        <Typography className="search-results-empty">
          No results found. Try a different search term.
        </Typography>
      )}

      {channels.length > 0 && (
        <Box className="search-results-section">
          <Typography variant="h6" className="search-results-section-title">
            Channels
          </Typography>
          <Stack direction="row" className="search-results-channels">
            {channels.map((user) => (
              <Link
                key={user.id}
                to={`/channel/${user.id}`}
                className="search-results-channel-link"
              >
                <Stack direction="column" alignItems="center" className="search-results-channel-card">
                  <Avatar
                    sx={{
                      bgcolor: stringToColor(user.username),
                      width: 56,
                      height: 56,
                      fontSize: 24,
                    }}
                  >
                    {(user.username || '?').charAt(0).toUpperCase()}
                  </Avatar>
                  <Typography variant="subtitle2" className="search-results-channel-name">
                    {user.username}
                    <CheckCircle style={{ fontSize: 10, color: "gray", marginLeft: 4 }} />
                  </Typography>
                </Stack>
              </Link>
            ))}
          </Stack>
        </Box>
      )}

      {videos.length > 0 && (
        <Box className="search-results-section">
          <Typography variant="h6" className="search-results-section-title">
            Videos
          </Typography>
          <Stack direction="row" className="search-results-categories">
            <button
              className={`category-btn${selectedCategory === null ? ' selected' : ''}`}
              onClick={() => setSelectedCategory(null)}
            >
              All
            </button>
            {categories.filter(c => c.name !== "New Videos").map((cat) => (
              <button
                key={cat.name}
                className={`category-btn${selectedCategory === cat.name ? ' selected' : ''}`}
                onClick={() => setSelectedCategory(selectedCategory === cat.name ? null : cat.name)}
              >
                <span>{cat.icon}</span>
                <span>{cat.name}</span>
              </button>
            ))}
          </Stack>
          {filteredVideos.length > 0 ? (
            <Videos videos={filteredVideos} />
          ) : (
            <Typography className="search-results-empty">
              No videos in this category.
            </Typography>
          )}
        </Box>
      )}
    </Box>
  );
}

export default SearchResults;
