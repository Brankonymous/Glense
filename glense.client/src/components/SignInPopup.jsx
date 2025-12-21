/* eslint-disable react/prop-types */
import { useState } from "react";
import {
  Modal,
  Box,
  Tab,
  Tabs,
  Typography,
  TextField,
  Button,
  IconButton,
  Alert,
  CircularProgress
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { useAuth } from "../context/AuthContext";
import "../css/SignInPopup.css";

const SignInPopup = ({ open, onClose }) => {
  const [activeTab, setActiveTab] = useState(0);
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    password: "",
  });
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const { login, register } = useAuth();

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
    setFormData({ username: "", email: "", password: "" });
    setError("");
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
    setError("");
  };

  const handleSubmit = async () => {
    setError("");
    setIsLoading(true);

    try {
      let result;
      if (activeTab === 0) {
        // Sign In
        result = await login(formData.username, formData.password);
      } else {
        // Register
        if (!formData.email) {
          setError("Email is required for registration");
          setIsLoading(false);
          return;
        }
        result = await register(formData.username, formData.email, formData.password);
      }

      if (result.success) {
        onClose();
        setFormData({ username: "", email: "", password: "" });
      } else {
        setError(result.error);
      }
    } catch {
      setError("An unexpected error occurred. Please try again.");
    } finally {
      setIsLoading(false);
    }
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
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        <Box component="form" className="modal-form">
          <TextField
            className="modal-input"
            fullWidth
            label="Username"
            name="username"
            value={formData.username}
            onChange={handleInputChange}
            disabled={isLoading}
            required
          />
          {activeTab === 1 && (
            <TextField
              className="modal-input"
              fullWidth
              label="Email"
              name="email"
              type="email"
              value={formData.email}
              onChange={handleInputChange}
              disabled={isLoading}
              required
            />
          )}
          <TextField
            className="modal-input"
            fullWidth
            type="password"
            label="Password"
            name="password"
            value={formData.password}
            onChange={handleInputChange}
            disabled={isLoading}
            required
          />
          <Button
            className="modal-submit-button"
            fullWidth
            variant="contained"
            onClick={handleSubmit}
            disabled={isLoading}
          >
            {isLoading ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              activeTab === 0 ? "Sign In" : "Register"
            )}
          </Button>
        </Box>
      </Box>
    </Modal>
  );
};

export default SignInPopup;
