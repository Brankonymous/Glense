/* eslint-disable react/prop-types */
import { useState, useRef } from "react";
import { Box, TextField, Button, Typography, Stack } from "@mui/material";
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import ImageIcon from '@mui/icons-material/Image';
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { uploadVideo } from "../utils/videoApi";

import "../css/Upload.css";

function Upload() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const videoRef = useRef(null);
  const thumbRef = useRef(null);

  const [file, setFile] = useState(null);
  const [thumbnail, setThumbnail] = useState(null);
  const [thumbPreview, setThumbPreview] = useState(null);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState("error");

  const handleThumbnail = (e) => {
    const img = e.target.files?.[0];
    if (!img) { setThumbnail(null); setThumbPreview(null); return; }
    const url = URL.createObjectURL(img);
    const image = new Image();
    image.onload = () => {
      const ratio = image.width / image.height;
      if (ratio < 1.6 || ratio > 1.9) {
        URL.revokeObjectURL(url);
        setMessage("Thumbnail must be 16:9 aspect ratio (e.g. 1280x720)");
        setMessageType("error");
        setThumbnail(null);
        setThumbPreview(null);
        e.target.value = '';
      } else {
        setMessage("");
        setThumbnail(img);
        setThumbPreview(url);
      }
    };
    image.src = url;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file || !thumbnail) {
      setMessage("Please select both a video file and a thumbnail image.");
      setMessageType("error");
      return;
    }
    setLoading(true);
    setMessage("");
    try {
      const resp = await uploadVideo(file, title, description, user?.id || '', thumbnail);
      setMessage("Upload successful!");
      setMessageType("success");
      if (resp?.id) {
        navigate(`/video/${resp.id}`);
      } else {
        setLoading(false);
      }
    } catch (err) {
      setMessage(err.message || String(err));
      setMessageType("error");
      setLoading(false);
    }
  };

  return (
    <Box className="upload-page">
      <Box className="upload-form" component="form" onSubmit={handleSubmit}>
        <Typography variant="h5">Upload Video</Typography>

        <Stack spacing={2}>
          <Box>
            <input ref={videoRef} accept="video/*" type="file" hidden onChange={(e) => setFile(e.target.files?.[0] || null)} />
            <Button
              variant="outlined"
              startIcon={<CloudUploadIcon />}
              onClick={() => videoRef.current?.click()}
              fullWidth
              className="upload-file-btn"
            >
              {file ? file.name : "Choose video file"}
            </Button>
          </Box>

          <Box>
            <input ref={thumbRef} accept="image/*" type="file" hidden onChange={handleThumbnail} />
            <Button
              variant="outlined"
              startIcon={<ImageIcon />}
              onClick={() => thumbRef.current?.click()}
              fullWidth
              className="upload-file-btn"
            >
              {thumbnail ? thumbnail.name : "Choose thumbnail (16:9)"}
            </Button>
            {thumbPreview && (
              <Box className="upload-thumb-preview">
                <img src={thumbPreview} alt="Thumbnail preview" />
              </Box>
            )}
          </Box>
        </Stack>

        <TextField label="Title" value={title} onChange={(e) => setTitle(e.target.value)} fullWidth margin="normal" />
        <TextField label="Description" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth multiline rows={4} margin="normal" />

        <Button variant="contained" color="primary" type="submit" disabled={loading || !file || !thumbnail}>
          {loading ? "Uploading..." : "Upload"}
        </Button>

        {message && (
          <Typography sx={{ mt: 1, color: messageType === "success" ? "#4caf50" : "#f44336" }}>
            {message}
          </Typography>
        )}
      </Box>
    </Box>
  );
}

export default Upload;
