import { useState, useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack } from "@mui/material";
import {
  CheckCircle,
  ThumbDownOutlined,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos } from ".";
import { videos } from "../utils/constants";

function VideoStream() {
  const [videoStream, setvideoStream] = useState(null);

  const [showMoreTags, setShowMoreTags] = useState(false);
  const [showMoreDesc, setShowMoreDesc] = useState(false);

  // const { id } = useParams();
  const id = 'haDjmBT9tu4';

  const publishedAt = 'Nov 22, 2024';
  const channelId = 'mkbhd';
  const title = 'An Honest Review of Apple Intelligence... So Far';
  const description = 'Reviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\n';
  const channelTitle = 'Marques Brownlee';
  const tags = ['Apple'];
  const viewCount = 2364175;
  const likeCount = 123456;

  return (
    <Box minHeight='95vh' width="100vw" height="100vh">
      <Stack direction="row" sx={{ p: "3rem" }}>
        <Box         
          sx={{
            width: '50%',
            marginLeft: '20%',
          }}
        >
          <Box
            sx={{ width: "100%", position: "sticky", top: "86px", zIndex: "1" }}
          >
            <ReactPlayer
              width='100%'
              url={`https://www.youtube.com/watch?v=${id}`}
              controls
            />
            <Typography
              color='#fff'
              variant='h5'
              fontWeight='bold'
              sx={{ color: "#fff" }}
              py={2}
              px={2}
            >
              {title}
            </Typography>
            <Stack
              direction='row'
              alignItems='center'
              gap='.5rem'
              sx={{ color: "#fff" }}
              py={1}
              px={2}
            >
              <Link to={`/channel/${channelId}`}>
                <Typography
                  whiteSpace='nowrap'
                  display='flex'
                  alignItems='center'
                  color='gray'
                  fontWeight='bold'
                >
                  {channelTitle}
                  <CheckCircle
                    sx={{
                      fontSize: "12px",
                      color: "gray",
                      ml: "5px",
                    }}
                  />
                </Typography>
              </Link>

              <Typography
                variant='body1'
                fontSize={{ xs: "10px", sm: "1rem" }}
                sx={{
                  opacity: 0.7,
                  backgroundColor: "rgba(255, 255, 255, 0.1)",
                  p: ".5rem",
                  borderRadius: "30px",
                  display: "flex",
                  alignItems: "center",
                  gap: "5px",
                }}
              >
                <ThumbUpOutlined
                  sx={{
                    fontSize: "20px",
                    color: "gray",
                  }}
                />
                {Number(likeCount).toLocaleString()} {""}
                {"|"}
                {""}
                <ThumbDownOutlined
                  sx={{
                    fontSize: "20px",
                    color: "gray",
                  }}
                />
              </Typography>
            </Stack>
            {/* Description */}
            <Box
              sx={{
                backgroundColor: "rgba(255, 255, 255, 0.1)",
                borderRadius: "30px",
                p: "15px",
                marginTop: "10px",
              }}
            >
              <Box sx={{ color: "gray" }}>
                <Typography>
                  {Number(viewCount).toLocaleString()} views
                </Typography>
                <Typography sx={{ fontWeight: "bold" }}>
                  Published at {publishedAt}
                </Typography>

                {tags.map((tag) =>
                  tags.length > 10 ? (
                    <Typography
                      sx={{ display: "inline-block", color: "#3366CC" }}
                    >
                      {showMoreTags ? tag : `#${tag.substring(0, 5)}`}
                    </Typography>
                  ) : (
                    <Typography
                      sx={{ display: "inline-block", color: "#3366CC" }}
                    >
                      # {tags}
                    </Typography>
                  )
                )}
                {tags.length > 10 && (
                  <button
                    style={{
                      outline: "none",
                      border: "none",
                      display: "inline",
                      fontSize: "14px",
                      backgroundColor: "transparent",
                      color: "#3366CC",
                    }}
                    onClick={() => setShowMoreTags(!showMoreTags)}
                  >
                    {showMoreTags ? "Show less" : "..."}
                  </button>
                )}

                <Typography
                  sx={{
                    color: "#fff",
                  }}
                >
                  {showMoreDesc
                    ? description
                    : `${description.substring(0, 250)}`}
                  <button
                    style={{
                      outline: "none",
                      border: "none",
                      fontSize: "14px",
                      backgroundColor: "transparent",
                      color: "rgba(255,255,255,.8)",
                    }}
                    onClick={() => setShowMoreDesc(!showMoreDesc)}
                  >
                    {showMoreDesc ? "Show less" : "Show more"}
                  </button>
                </Typography>
              </Box>
            </Box>
            {/* Comments section */}
            <Typography sx={{ color: "#f1f1f1", my: "1rem", mx: "1rem" }}>
              {" "}
              Comments
            </Typography>
          </Box>
        </Box>

        <Box
          sx={{
            width: '50%',
            display: 'flex',
            marginLeft: '3%',
          }}
        >
          <Videos videos={videos} direction={'column'} />
        </Box>
      </Stack>
    </Box>
  );
}

export default VideoStream;