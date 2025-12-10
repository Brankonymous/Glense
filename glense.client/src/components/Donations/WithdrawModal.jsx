import { useState } from "react";
import {
    Modal,
    Box,
    Typography,
    TextField,
    Button,
    IconButton
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import "../../css/Donations/WithdrawModal.css";

function WithdrawModal({ open, onClose, onSubmit, currentBalance }) {
    const [amount, setAmount] = useState("");
    const [showConfirm, setShowConfirm] = useState(false);

    const handleAmountChange = (e) => {
        const value = e.target.value;
        if (value === "" || /^\d+\.?\d{0,2}$/.test(value)) {
            setAmount(value);
        }
    };

    const handleWithdrawAll = () => {
        setAmount(currentBalance.toString());
    };

    const handleSubmit = () => {
        const withdrawAmount = parseFloat(amount);
        if (!amount || withdrawAmount <= 0 || withdrawAmount > currentBalance) return;
        
        if (!showConfirm) {
            setShowConfirm(true);
            return;
        }

        onSubmit({
            amount: withdrawAmount,
            type: "withdraw",
            timestamp: new Date().toISOString()
        });
        resetForm();
    };

    const resetForm = () => {
        setAmount("");
        setShowConfirm(false);
    };

    const handleClose = () => {
        resetForm();
        onClose();
    };

    const handleBack = () => {
        setShowConfirm(false);
    };

    const isValidAmount = amount && parseFloat(amount) > 0 && parseFloat(amount) <= currentBalance;

    return (
        <Modal open={open} onClose={handleClose}>
            <Box className="withdraw-modal-box">
                <IconButton className="withdraw-modal-close" onClick={handleClose}>
                    <CloseIcon />
                </IconButton>

                {!showConfirm ? (
                    <>
                        <div className="withdraw-modal-header">
                            <Typography variant="h5" className="withdraw-modal-title">
                                Withdraw Funds
                            </Typography>
                            <Typography className="withdraw-modal-subtitle">
                                Transfer money to your bank account
                            </Typography>
                        </div>

                        <div className="balance-info">
                            <span className="balance-label">Available Balance</span>
                            <span className="balance-value">${currentBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
                        </div>

                        <div className="amount-section">
                            <Typography className="section-label">Withdraw Amount</Typography>
                            <TextField
                                placeholder="Enter amount"
                                value={amount}
                                onChange={handleAmountChange}
                                className="withdraw-input"
                                variant="outlined"
                                fullWidth
                                InputProps={{
                                    startAdornment: <span className="currency-symbol">$</span>
                                }}
                            />
                            <button className="withdraw-all-btn" onClick={handleWithdrawAll}>
                                Withdraw All
                            </button>
                            {amount && parseFloat(amount) > currentBalance && (
                                <Typography className="error-text">
                                    Amount exceeds available balance
                                </Typography>
                            )}
                        </div>

                        <Button
                            className="withdraw-submit-btn"
                            fullWidth
                            variant="contained"
                            onClick={handleSubmit}
                            disabled={!isValidAmount}
                        >
                            Continue
                        </Button>
                    </>
                ) : (
                    <div className="confirmation-view">
                        <div className="withdraw-modal-header">
                            <Typography variant="h5" className="withdraw-modal-title">
                                Confirm Withdrawal
                            </Typography>
                        </div>

                        <div className="confirmation-details">
                            <div className="confirm-amount">
                                <span className="amount-label">You're withdrawing</span>
                                <span className="amount-value withdraw">${parseFloat(amount).toFixed(2)}</span>
                            </div>
                            <div className="remaining-balance">
                                <span className="remaining-label">Remaining balance</span>
                                <span className="remaining-value">
                                    ${(currentBalance - parseFloat(amount)).toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                </span>
                            </div>
                            <Typography className="confirm-note">
                                Funds will be transferred within 1-3 business days
                            </Typography>
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
                                className="confirm-btn withdraw"
                                variant="contained"
                                onClick={handleSubmit}
                            >
                                Withdraw ${parseFloat(amount).toFixed(2)}
                            </Button>
                        </div>
                    </div>
                )}
            </Box>
        </Modal>
    );
}

export default WithdrawModal;

