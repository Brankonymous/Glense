import { Box, CardContent, Typography, CardMedia, Stack } from "@mui/material";
import { CheckCircle } from "@mui/icons-material";
import { Link } from "react-router-dom";
import { demoProfilePicture } from "../utils/constants";

import "../css/ChannelCard.css";

function ChannelCard({ channelDetail, marginTop }) {
  return (
    <Stack
      direction="column"
      className="channel-card-container"
      style={{ marginTop }}
    >
      <Link
        to={`${
          channelDetail?.id.channelId
            ? `/channel/${channelDetail?.id?.channelId}`
            : ``
        }`}
        className="channel-card-link"
      >
        <CardContent className="channel-card-content">
          <CardMedia
            image={demoProfilePicture}
            className="channel-card-media"
          />

          <Typography variant="h6" className="channel-card-title">
            Beyn
            <CheckCircle style={{ fontSize: 12, color: "gray", marginLeft: "5px" }} />
          </Typography>

          <Stack
            direction={{ xs: "column", sm: "row" }}
            className="channel-card-info"
          >
            <Typography>@bane.grbic</Typography>
        
            <Typography>
            {parseInt(
                2400000
            ).toLocaleString("en-US")}{" "}
            Subscribers
            </Typography>
            <Typography>
                5 Videos
            </Typography>
          </Stack>

            <Typography className="channel-card-description">
              Naj jaci producent u svojoj sobi
            </Typography>
        </CardContent>
      </Link>
    </Stack>
  );
}

export default ChannelCard;
