import { useState } from "react";
import {
    Modal,
    Box,
    Typography,
    TextField,
    Button,
    IconButton,
    Autocomplete,
    Avatar
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { users, recentRecipients } from "../../utils/constants";
import "../../css/Donations/DonationModal.css";

const PRESET_AMOUNTS = [5, 10, 25, 50, 100];

function DonationModal({ open, onClose, onSubmit }) {
    const [selectedUser, setSelectedUser] = useState(null);
    const [amount, setAmount] = useState("");
    const [message, setMessage] = useState("");
    const [showConfirm, setShowConfirm] = useState(false);

    const handlePresetAmount = (preset) => {
        setAmount(preset.toString());
    };

    const handleAmountChange = (e) => {
        const value = e.target.value;
        if (value === "" || /^\d+$/.test(value)) {
            setAmount(value);
        }
    };

    const handleSubmit = () => {
        if (!selectedUser || !amount || parseInt(amount) <= 0) return;
        
        if (!showConfirm) {
            setShowConfirm(true);
            return;
        }

        const donation = {
            recipientId: selectedUser.id,
            recipientName: selectedUser.name,
            amount: parseInt(amount),
            message: message,
            donatedAt: new Date().toISOString()
        };

        onSubmit(donation);
        resetForm();
    };

    const resetForm = () => {
        setSelectedUser(null);
        setAmount("");
        setMessage("");
        setShowConfirm(false);
    };

    const handleClose = () => {
        resetForm();
        onClose();
    };

    const handleBack = () => {
        setShowConfirm(false);
    };

    return (
        <Modal open={open} onClose={handleClose}>
            <Box className="donation-modal-box">
                <IconButton className="donation-modal-close" onClick={handleClose}>
                    <CloseIcon />
                </IconButton>

                {!showConfirm ? (
                    <>
                        <div className="donation-modal-header">
                            <Typography variant="h5" className="donation-modal-title">
                                Send a Donation
                            </Typography>
                            <Typography className="donation-modal-subtitle">
                                Support your favorite creators
                            </Typography>
                        </div>

                        {/* Recent Recipients */}
                        {recentRecipients.length > 0 && (
                            <div className="recent-recipients">
                                <Typography className="section-label">Quick Send</Typography>
                                <div className="recent-list">
                                    {recentRecipients.map((user) => (
                                        <button
                                            key={user.id}
                                            className={`recent-btn ${selectedUser?.id === user.id ? "selected" : ""}`}
                                            onClick={() => setSelectedUser(user)}
                                        >
                                            <Avatar 
                                                src={user.profileImage} 
                                                className="recent-avatar"
                                            />
                                            <span className="recent-name">{user.name}</span>
                                        </button>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* User Search */}
                        <div className="user-search-section">
                            <Typography className="section-label">Recipient</Typography>
                            <Autocomplete
                                options={users}
                                getOptionLabel={(option) => option.name}
                                value={selectedUser}
                                onChange={(e, newValue) => setSelectedUser(newValue)}
                                renderOption={(props, option) => (
                                    <Box component="li" {...props} className="user-option">
                                        <Avatar 
                                            src={option.profileImage} 
                                            sx={{ width: 32, height: 32, mr: 1.5 }}
                                        />
                                        <div className="user-option-info">
                                            <span className="user-option-name">{option.name}</span>
                                            <span className="user-option-handle">@{option.handle}</span>
                                        </div>
                                    </Box>
                                )}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        placeholder="Search users..."
                                        className="donation-input"
                                        variant="outlined"
                                    />
                                )}
                            />
                        </div>

                        {/* Amount Selection */}
                        <div className="amount-section">
                            <Typography className="section-label">Amount</Typography>
                            <div className="preset-amounts">
                                {PRESET_AMOUNTS.map((preset) => (
                                    <button
                                        key={preset}
                                        className={`preset-btn ${amount === preset.toString() ? "selected" : ""}`}
                                        onClick={() => handlePresetAmount(preset)}
                                    >
                                        ${preset}
                                    </button>
                                ))}
                            </div>
                            <TextField
                                placeholder="Or enter custom amount"
                                value={amount}
                                onChange={handleAmountChange}
                                className="donation-input amount-input"
                                variant="outlined"
                                InputProps={{
                                    startAdornment: <span className="currency-symbol">$</span>
                                }}
                            />
                        </div>

                        {/* Optional Message */}
                        <div className="message-section">
                            <Typography className="section-label">
                                Message <span className="optional-tag">(optional)</span>
                            </Typography>
                            <TextField
                                placeholder="Add a personal note..."
                                value={message}
                                onChange={(e) => setMessage(e.target.value)}
                                className="donation-input message-input"
                                variant="outlined"
                                multiline
                                rows={2}
                                inputProps={{ maxLength: 200 }}
                            />
                            <span className="char-count">{message.length}/200</span>
                        </div>

                        <Button
                            className="donation-submit-btn"
                            fullWidth
                            variant="contained"
                            onClick={handleSubmit}
                            disabled={!selectedUser || !amount || parseInt(amount) <= 0}
                        >
                            Continue
                        </Button>
                    </>
                ) : (
                    /* Confirmation View */
                    <div className="confirmation-view">
                        <div className="donation-modal-header">
                            <Typography variant="h5" className="donation-modal-title">
                                Confirm Donation
                            </Typography>
                        </div>

                        <div className="confirmation-details">
                            <div className="confirm-recipient">
                                <Avatar 
                                    src={selectedUser?.profileImage} 
                                    className="confirm-avatar"
                                />
                                <div className="confirm-user-info">
                                    <span className="confirm-name">{selectedUser?.name}</span>
                                    <span className="confirm-handle">@{selectedUser?.handle}</span>
                                </div>
                            </div>

                            <div className="confirm-amount">
                                <span className="amount-label">Amount</span>
                                <span className="amount-value">${amount}</span>
                            </div>

                            {message && (
                                <div className="confirm-message">
                                    <span className="message-label">Message</span>
                                    <p className="message-text">"{message}"</p>
                                </div>
                            )}
                        </div>

                        <div className="confirmation-actions">
                            <Button
                                className="back-btn"
                                variant="outlined"
                                onClick={handleBack}
                            >
                                Back
                            </Button>
                            <Button
                                className="confirm-btn"
                                variant="contained"
                                onClick={handleSubmit}
                            >
                                Send ${amount}
                            </Button>
                        </div>
                    </div>
                )}
            </Box>
        </Modal>
    );
}

export default DonationModal;

