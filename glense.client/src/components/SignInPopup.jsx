import React, { useState } from "react";
import {
  Modal,
  Box,
  Tab,
  Tabs,
  Typography,
  TextField,
  Button,
  IconButton
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import "../css/SignInPopup.css"; 

const SignInPopup = ({ open, onClose }) => {
  const [activeTab, setActiveTab] = useState(0); 
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
    setFormData({ username: "", password: "" }); 
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleSubmit = () => {
    if (activeTab === 0) {
      console.log("Signing In with", formData);
    } else {
      console.log("Registering with", formData);
    }
    onClose(); 
  };

  return (
    <Modal open={open} onClose={onClose}>
      <Box className="modal-box">
        <IconButton className="modal-close-button" onClick={onClose}>
          <CloseIcon />
        </IconButton>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          centered
          className="modal-tabs"
        >
          <Tab className="sign-in-tab" label="Sign In" />
          <Tab className="sign-in-tab" label="Register" />
        </Tabs>
        <Typography variant="h6" className="modal-title">
          {activeTab === 0 ? "Sign In" : "Register"}
        </Typography>
        <Box component="form" className="modal-form">
          <TextField
            className="modal-input"
            fullWidth
            label="Username"
            name="username"
            value={formData.username}
            onChange={handleInputChange}
          />
          <TextField
            className="modal-input"
            fullWidth
            type="password"
            label="Password"
            name="password"
            value={formData.password}
            onChange={handleInputChange}
          />
          <Button
            className="modal-submit-button"
            fullWidth
            variant="contained"
            onClick={handleSubmit}
          >
            {activeTab === 0 ? "Sign In" : "Register"}
          </Button>
        </Box>
      </Box>
    </Modal>
  );
};

export default SignInPopup;
