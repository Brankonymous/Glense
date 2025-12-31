import { useState } from "react";
import {
    Modal,
    Box,
    Typography,
    TextField,
    Button,
    IconButton,
    CircularProgress
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import "../../css/Donations/DepositModal.css";

const PRESET_AMOUNTS = [10, 25, 50, 100, 250];

function DepositModal({ open, onClose, onSubmit, isSubmitting = false }) {
    const [amount, setAmount] = useState("");
    const [showConfirm, setShowConfirm] = useState(false);

    const handlePresetAmount = (preset) => {
        setAmount(preset.toString());
    };

    const handleAmountChange = (e) => {
        const value = e.target.value;
        if (value === "" || /^\d+\.?\d{0,2}$/.test(value)) {
            setAmount(value);
        }
    };

    const handleSubmit = () => {
        if (!amount || parseFloat(amount) <= 0) return;
        
        if (!showConfirm) {
            setShowConfirm(true);
            return;
        }

        onSubmit({
            amount: parseFloat(amount),
            type: "deposit",
            timestamp: new Date().toISOString()
        });
        // Don't reset form here - let parent handle it on success
    };

    const resetForm = () => {
        setAmount("");
        setShowConfirm(false);
    };

    const handleClose = () => {
        if (isSubmitting) return;
        resetForm();
        onClose();
    };

    const handleBack = () => {
        setShowConfirm(false);
    };

    return (
        <Modal open={open} onClose={handleClose}>
            <Box className="deposit-modal-box">
                <IconButton 
                    className="deposit-modal-close" 
                    onClick={handleClose}
                    disabled={isSubmitting}
                >
                    <CloseIcon />
                </IconButton>

                {!showConfirm ? (
                    <>
                        <div className="deposit-modal-header">
                            <Typography variant="h5" className="deposit-modal-title">
                                Deposit Funds
                            </Typography>
                            <Typography className="deposit-modal-subtitle">
                                Add money to your balance
                            </Typography>
                        </div>

                        <div className="amount-section">
                            <Typography className="section-label">Select Amount</Typography>
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
                                className="deposit-input"
                                variant="outlined"
                                fullWidth
                                InputProps={{
                                    startAdornment: <span className="currency-symbol">$</span>
                                }}
                            />
                        </div>

                        <Button
                            className="deposit-submit-btn"
                            fullWidth
                            variant="contained"
                            onClick={handleSubmit}
                            disabled={!amount || parseFloat(amount) <= 0}
                        >
                            Continue
                        </Button>
                    </>
                ) : (
                    <div className="confirmation-view">
                        <div className="deposit-modal-header">
                            <Typography variant="h5" className="deposit-modal-title">
                                Confirm Deposit
                            </Typography>
                        </div>

                        <div className="confirmation-details">
                            <div className="confirm-amount">
                                <span className="amount-label">You're depositing</span>
                                <span className="amount-value">${parseFloat(amount).toFixed(2)}</span>
                            </div>
                            <Typography className="confirm-note">
                                Funds will be added to your balance immediately
                            </Typography>
                        </div>

                        <div className="confirmation-actions">
                            <Button
                                className="back-btn"
                                variant="outlined"
                                onClick={handleBack}
                                disabled={isSubmitting}
                            >
                                Back
                            </Button>
                            <Button
                                className="confirm-btn deposit"
                                variant="contained"
                                onClick={handleSubmit}
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? (
                                    <>
                                        <CircularProgress size={20} color="inherit" sx={{ mr: 1 }} />
                                        Processing...
                                    </>
                                ) : (
                                    `Deposit $${parseFloat(amount).toFixed(2)}`
                                )}
                            </Button>
                        </div>
                    </div>
                )}
            </Box>
        </Modal>
    );
}

export default DepositModal;
