import { Avatar } from "@mui/material";
import "../../css/Donations/DonationHistory.css";

function DonationHistory({ donations, filter }) {
    const currentUserId = 1; // Mock current user ID

    const filteredDonations = donations.filter((donation) => {
        if (filter === "sent") return donation.donatorId === currentUserId;
        if (filter === "received") return donation.recipientId === currentUserId;
        return true; // "history" shows all
    });

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

        if (diffDays === 0) {
            return "Today";
        } else if (diffDays === 1) {
            return "Yesterday";
        } else if (diffDays < 7) {
            return `${diffDays} days ago`;
        } else {
            return date.toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
                year: date.getFullYear() !== now.getFullYear() ? "numeric" : undefined
            });
        }
    };

    const formatTime = (dateString) => {
        return new Date(dateString).toLocaleTimeString("en-US", {
            hour: "numeric",
            minute: "2-digit",
            hour12: true
        });
    };

    if (filteredDonations.length === 0) {
        return (
            <div className="donation-history-empty">
                <span className="empty-icon">📭</span>
                <p className="empty-text">
                    {filter === "sent" && "You haven't sent any donations yet"}
                    {filter === "received" && "You haven't received any donations yet"}
                    {filter === "history" && "No donation history yet"}
                </p>
                <p className="empty-subtext">
                    Support your favorite creators by sending them a donation!
                </p>
            </div>
        );
    }

    return (
        <div className="donation-history">
            {filteredDonations.map((donation, index) => {
                const isSent = donation.donatorId === currentUserId;
                const otherUser = isSent ? donation.recipient : donation.donator;
                
                return (
                    <div key={index} className={`donation-item ${isSent ? "sent" : "received"}`}>
                        <div className="donation-item-left">
                            <Avatar 
                                src={otherUser.profileImage} 
                                className="donation-avatar"
                            />
                            <div className="donation-info">
                                <div className="donation-user-row">
                                    <span className="donation-direction">
                                        {isSent ? "Sent to" : "Received from"}
                                    </span>
                                    <span className="donation-user-name">{otherUser.name}</span>
                                </div>
                                {donation.message && (
                                    <p className="donation-message">"{donation.message}"</p>
                                )}
                                <span className="donation-time">
                                    {formatDate(donation.donatedAt)} at {formatTime(donation.donatedAt)}
                                </span>
                            </div>
                        </div>
                        <div className="donation-item-right">
                            <span className={`donation-amount ${isSent ? "negative" : "positive"}`}>
                                {isSent ? "-" : "+"}${donation.amount}
                            </span>
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

export default DonationHistory;

