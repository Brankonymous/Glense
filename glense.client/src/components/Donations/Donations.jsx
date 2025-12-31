import { useState, useEffect, useCallback } from "react";
import { CircularProgress, Alert, Snackbar } from "@mui/material";
import DonationModal from "./DonationModal";
import DepositModal from "./DepositModal";
import WithdrawModal from "./WithdrawModal";
import DonationHistory from "./DonationHistory";
import { users } from "../../utils/constants";
import { 
    getOrCreateWallet, 
    getAllDonations, 
    createDonation, 
    topUpWallet,
    withdrawFromWallet,
    transformDonation,
    createUsersMap 
} from "../../utils/donationApi";
import logo from "../../assets/logo_transparent.png";
import "../../css/Donations/Donations.css";

// Create a map of users for quick lookup
const usersMap = createUsersMap(users);

// TODO: Get this from auth context
const CURRENT_USER_ID = 1;

function Donations() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isDepositOpen, setIsDepositOpen] = useState(false);
    const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);
    const [activeTab, setActiveTab] = useState("history");
    
    // Data state
    const [balance, setBalance] = useState(0);
    const [donations, setDonations] = useState([]);
    
    // Loading states
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    // Error/notification state
    const [notification, setNotification] = useState({ open: false, message: '', severity: 'success' });

    // Fetch wallet and donations on mount
    const fetchData = useCallback(async () => {
        setIsLoading(true);
        try {
            // Fetch wallet and donations in parallel
            const [walletData, donationsData] = await Promise.all([
                getOrCreateWallet(CURRENT_USER_ID),
                getAllDonations(CURRENT_USER_ID)
            ]);

            setBalance(walletData.balance);
            
            // Transform donations to frontend format
            const transformedDonations = donationsData.all.map(d => 
                transformDonation(d, usersMap, CURRENT_USER_ID)
            );
            setDonations(transformedDonations);
        } catch (error) {
            console.error('Error fetching data:', error);
            showNotification('Failed to load data. Please try again.', 'error');
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const showNotification = (message, severity = 'success') => {
        setNotification({ open: true, message, severity });
    };

    const handleCloseNotification = () => {
        setNotification(prev => ({ ...prev, open: false }));
    };

    const handleDonationSuccess = async (donationData) => {
        setIsSubmitting(true);
        try {
            const result = await createDonation({
                donorUserId: CURRENT_USER_ID,
                recipientUserId: donationData.recipientId,
                amount: donationData.amount,
                message: donationData.message
            });

            // Update local state
            setBalance(prev => prev - donationData.amount);
            
            // Add new donation to history
            const newDonation = transformDonation(result, usersMap, CURRENT_USER_ID);
            setDonations(prev => [newDonation, ...prev]);
            
            setIsModalOpen(false);
            showNotification(`Successfully sent $${donationData.amount} to ${donationData.recipientName}!`);
        } catch (error) {
            console.error('Error creating donation:', error);
            showNotification(error.message || 'Failed to send donation. Please try again.', 'error');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDepositSuccess = async (deposit) => {
        setIsSubmitting(true);
        try {
            const result = await topUpWallet(CURRENT_USER_ID, deposit.amount);
            setBalance(result.balance);
            setIsDepositOpen(false);
            showNotification(`Successfully deposited $${deposit.amount.toFixed(2)}!`);
        } catch (error) {
            console.error('Error depositing:', error);
            showNotification(error.message || 'Failed to deposit. Please try again.', 'error');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleWithdrawSuccess = async (withdraw) => {
        setIsSubmitting(true);
        try {
            const result = await withdrawFromWallet(CURRENT_USER_ID, withdraw.amount);
            setBalance(result.balance);
            setIsWithdrawOpen(false);
            showNotification(`Successfully withdrew $${withdraw.amount.toFixed(2)}!`);
        } catch (error) {
            console.error('Error withdrawing:', error);
            showNotification(error.message || 'Failed to withdraw. Please try again.', 'error');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isLoading) {
        return (
            <div className="donations-container">
                <div className="donations-loading">
                    <CircularProgress size={48} />
                    <p>Loading your wallet...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="donations-container">
            <div className="donations-header">
                <div className="header-left">
                    <h1 className="donations-title">
                        <img src={logo} alt="Glense" className="title-icon" />
                        Donations
                    </h1>
                    <div className="balance-display">
                        <span className="balance-label">Balance: </span>
                        <span className="balance-amount">${balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
                    </div>
                </div>
                <div className="header-actions">
                    <button 
                        className="action-btn deposit-btn"
                        onClick={() => setIsDepositOpen(true)}
                        disabled={isSubmitting}
                    >
                        <span className="btn-icon">💰</span>
                        Deposit
                    </button>
                    <button 
                        className="action-btn withdraw-btn"
                        onClick={() => setIsWithdrawOpen(true)}
                        disabled={isSubmitting || balance <= 0}
                    >
                        <span className="btn-icon">💸</span>
                        Withdraw
                    </button>
                    <button 
                        className="action-btn send-btn"
                        onClick={() => setIsModalOpen(true)}
                        disabled={isSubmitting || balance <= 0}
                    >
                        <span className="btn-icon">✨</span>
                        Send Donation
                    </button>
                </div>
            </div>

            <div className="donations-tabs">
                <button 
                    className={`tab-btn ${activeTab === "history" ? "active" : ""}`}
                    onClick={() => setActiveTab("history")}
                >
                    All History
                </button>
                <button 
                    className={`tab-btn ${activeTab === "sent" ? "active" : ""}`}
                    onClick={() => setActiveTab("sent")}
                >
                    Sent
                </button>
                <button 
                    className={`tab-btn ${activeTab === "received" ? "active" : ""}`}
                    onClick={() => setActiveTab("received")}
                >
                    Received
                </button>
            </div>

            <DonationHistory 
                donations={donations} 
                filter={activeTab}
                currentUserId={CURRENT_USER_ID}
            />

            <DonationModal 
                open={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleDonationSuccess}
                isSubmitting={isSubmitting}
                currentBalance={balance}
            />

            <DepositModal 
                open={isDepositOpen}
                onClose={() => setIsDepositOpen(false)}
                onSubmit={handleDepositSuccess}
                isSubmitting={isSubmitting}
            />

            <WithdrawModal 
                open={isWithdrawOpen}
                onClose={() => setIsWithdrawOpen(false)}
                onSubmit={handleWithdrawSuccess}
                currentBalance={balance}
                isSubmitting={isSubmitting}
            />

            <Snackbar
                open={notification.open}
                autoHideDuration={5000}
                onClose={handleCloseNotification}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
            >
                <Alert 
                    onClose={handleCloseNotification} 
                    severity={notification.severity}
                    variant="filled"
                >
                    {notification.message}
                </Alert>
            </Snackbar>
        </div>
    );
}

export default Donations;
