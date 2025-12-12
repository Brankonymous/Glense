import { useState } from "react";
import DonationModal from "./DonationModal";
import DepositModal from "./DepositModal";
import WithdrawModal from "./WithdrawModal";
import DonationHistory from "./DonationHistory";
import { donationHistory } from "../../utils/constants";
import logo from "../../assets/logo_transparent.png";
import "../../css/Donations/Donations.css";

function Donations() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isDepositOpen, setIsDepositOpen] = useState(false);
    const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);
    const [activeTab, setActiveTab] = useState("history"); // "history" or "send"
    const [balance, setBalance] = useState(1250.75); // TODO: fetch from API

    const handleDonationSuccess = (donation) => {
        console.log("Donation sent:", donation);
        setBalance(prev => prev - donation.amount);
        setIsModalOpen(false);
    };

    const handleDepositSuccess = (deposit) => {
        console.log("Deposit:", deposit);
        setBalance(prev => prev + deposit.amount);
        setIsDepositOpen(false);
    };

    const handleWithdrawSuccess = (withdraw) => {
        console.log("Withdraw:", withdraw);
        setBalance(prev => prev - withdraw.amount);
        setIsWithdrawOpen(false);
    };

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
                    >
                        <span className="btn-icon">💰</span>
                        Deposit
                    </button>
                    <button 
                        className="action-btn withdraw-btn"
                        onClick={() => setIsWithdrawOpen(true)}
                    >
                        <span className="btn-icon">💸</span>
                        Withdraw
                    </button>
                    <button 
                        className="action-btn send-btn"
                        onClick={() => setIsModalOpen(true)}
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
                donations={donationHistory} 
                filter={activeTab}
            />

            <DonationModal 
                open={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleDonationSuccess}
            />

            <DepositModal 
                open={isDepositOpen}
                onClose={() => setIsDepositOpen(false)}
                onSubmit={handleDepositSuccess}
            />

            <WithdrawModal 
                open={isWithdrawOpen}
                onClose={() => setIsWithdrawOpen(false)}
                onSubmit={handleWithdrawSuccess}
                currentBalance={balance}
            />
        </div>
    );
}

export default Donations;
