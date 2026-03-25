/* eslint-disable react/prop-types */
import { useState } from "react";
import { Box, TextField, Button, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { uploadVideo } from "../utils/videoApi";

import "../css/Upload.css";

function Upload() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [file, setFile] = useState(null);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  const handleFile = (e) => {
    setFile(e.target.files?.[0] || null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file) {
      setMessage("Please select a file to upload.");
      return;
    }
    setLoading(true);
    setMessage("");
    try {
      const resp = await uploadVideo(file, title, description, user?.id || '');
      setMessage("Upload successful");
      // navigate to video page
      if (resp?.id) {
        navigate(`/video/${resp.id}`);
      } else {
        setLoading(false);
      }
    } catch (err) {
      setMessage(err.message || String(err));
      setLoading(false);
    }
  };

  return (
    <Box className="upload-page">
      <Box className="upload-form" component="form" onSubmit={handleSubmit}>
        <Typography variant="h5">Upload Video</Typography>

        <input accept="video/*" type="file" onChange={handleFile} />

        <TextField label="Title" value={title} onChange={(e) => setTitle(e.target.value)} fullWidth margin="normal" />
        <TextField label="Description" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth multiline rows={4} margin="normal" />

        <Button variant="contained" color="primary" type="submit" disabled={loading}>
          {loading ? "Uploading..." : "Upload"}
        </Button>

        {message && <Typography sx={{ mt: 2 }}>{message}</Typography>}
      </Box>
    </Box>
  );
}

export default Upload;
