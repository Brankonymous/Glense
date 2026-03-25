import { CardContent, Typography, Stack, Avatar } from "@mui/material";
import { CheckCircle } from "@mui/icons-material";

import "../css/ChannelCard.css";

const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
function stringToColor(str) {
  let h = 0;
  for (let i = 0; i < (str||'').length; i++) h = str.charCodeAt(i) + ((h << 5) - h);
  return colors[Math.abs(h) % colors.length];
}

function ChannelCard({ profile, videoCount = 0 }) {
  const username = profile?.username || 'Loading...';
  const email = profile?.email || '';
  const accountType = profile?.accountType || 'user';

  return (
    <Stack direction="column" className="channel-card-container">
      <CardContent className="channel-card-content">
        <Avatar
          sx={{
            bgcolor: stringToColor(username),
            width: 80,
            height: 80,
            fontSize: 32,
            margin: '0 auto 12px',
          }}
        >
          {username.charAt(0).toUpperCase()}
        </Avatar>

        <Typography variant="h6" className="channel-card-title">
          {username}
          <CheckCircle style={{ fontSize: 12, color: "gray", marginLeft: "5px" }} />
        </Typography>

        <Stack
          direction={{ xs: "column", sm: "row" }}
          className="channel-card-info"
        >
          <Typography>@{username}</Typography>
          <Typography>{accountType}</Typography>
          <Typography>{videoCount} Videos</Typography>
        </Stack>

        {email && (
          <Typography className="channel-card-description">
            {email}
          </Typography>
        )}
      </CardContent>
    </Stack>
  );
}

export default ChannelCard;
