import { useState } from "react";
import { useParams } from "react-router-dom";
import { Box, CardMedia, Typography } from "@mui/material";

import { Videos, ChannelCard } from ".";
import { videos } from "../utils/constants";

import "../css/ChannelDetail.css";

function ChannelDetail() {
  const [channelDetail, setChannelDetail] = useState(null);

  const channelBanner =
    channelDetail?.brandingSettings?.image?.bannerExternalUrl;

  return (
    <Box className="channel-detail-container">
      <Box>
        {channelBanner ? (
          <CardMedia
            image={channelBanner}
            className="channel-banner"
          />
        ) : (
          <div className="channel-fallback-banner" />
        )}

        <div className="channel-card-wrapper">
          <ChannelCard channelDetail={channelDetail} />
        </div>
      </Box>

      <Box className="recent-videos-section">
        <Typography className="recent-videos-title">
          Recent Videos
        </Typography>
        <Videos videos={videos} />
      </Box>
    </Box>
  );
}

export default ChannelDetail;
