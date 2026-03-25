import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Box, Typography } from "@mui/material";

import { Videos, ChannelCard } from ".";
import { getVideos } from "../utils/videoApi";
import { profileService } from "../services/profileService";

import "../css/ChannelDetail.css";

function ChannelDetail() {
  const { id } = useParams();
  const [profile, setProfile] = useState(null);
  const [videos, setVideos] = useState([]);

  useEffect(() => {
    let mounted = true;

    // Try to find user by username (search) or by GUID (direct)
    const isGuid = /^[0-9a-f]{8}-/.test(id);
    const fetchProfile = isGuid
      ? profileService.getUserById(id)
      : profileService.searchUsers(id, 1).then(users => users?.[0] || null);

    fetchProfile
      .then(p => { if (mounted && p) setProfile(p); })
      .catch(() => {});

    getVideos()
      .then(data => {
        if (!mounted || !Array.isArray(data)) return;
        // Filter videos by this channel's uploader
        const filtered = data.filter(v =>
          v.uploaderUsername === id || v.uploaderId === id
        );
        setVideos(filtered.length > 0 ? filtered : data);
      })
      .catch(() => {});

    return () => { mounted = false; };
  }, [id]);

  return (
    <Box className="channel-detail-container">
      <Box>
        <div className="channel-fallback-banner" />
        <div className="channel-card-wrapper">
          <ChannelCard profile={profile} videoCount={videos.length} />
        </div>
      </Box>

      <Box className="recent-videos-section">
        <Typography className="recent-videos-title">
          Videos
        </Typography>
        <Videos videos={videos} />
      </Box>
    </Box>
  );
}

export default ChannelDetail;
