/**
 * Donation Service API Client
 * Connects to the DonationService microservice
 */

const DONATION_API_BASE = import.meta.env.VITE_DONATION_API_URL || 'http://localhost:5100/api';

/**
 * Generic fetch wrapper with error handling
 */
async function apiFetch(endpoint, options = {}) {
    const url = `${DONATION_API_BASE}${endpoint}`;
    
    const defaultHeaders = {
        'Content-Type': 'application/json',
    };

    const config = {
        ...options,
        headers: {
            ...defaultHeaders,
            ...options.headers,
        },
    };

    try {
        const response = await fetch(url, config);
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `API error: ${response.status}`);
        }

        // Handle 204 No Content
        if (response.status === 204) {
            return null;
        }

        return await response.json();
    } catch (error) {
        console.error(`API Error [${endpoint}]:`, error);
        throw error;
    }
}

// ============================================
// WALLET API
// ============================================

/**
 * Get wallet by user ID
 * @param {number} userId 
 * @returns {Promise<{id: string, userId: number, balance: number, createdAt: string, updatedAt: string}>}
 */
export async function getWallet(userId) {
    return apiFetch(`/wallet/user/${userId}`);
}

/**
 * Create a new wallet for a user
 * @param {number} userId 
 * @param {number} initialBalance 
 * @returns {Promise<{id: string, userId: number, balance: number, createdAt: string, updatedAt: string}>}
 */
export async function createWallet(userId, initialBalance = 0) {
    return apiFetch('/wallet', {
        method: 'POST',
        body: JSON.stringify({ userId, initialBalance }),
    });
}

/**
 * Get or create wallet for user
 * @param {number} userId 
 * @returns {Promise<{id: string, userId: number, balance: number, createdAt: string, updatedAt: string}>}
 */
export async function getOrCreateWallet(userId) {
    try {
        return await getWallet(userId);
    } catch (error) {
        // Wallet doesn't exist, create it
        if (error.message.includes('not found') || error.message.includes('404')) {
            return await createWallet(userId, 0);
        }
        throw error;
    }
}

/**
 * Add funds to wallet (top-up/deposit)
 * @param {number} userId 
 * @param {number} amount 
 * @returns {Promise<{id: string, userId: number, balance: number, createdAt: string, updatedAt: string}>}
 */
export async function topUpWallet(userId, amount) {
    return apiFetch(`/wallet/user/${userId}/topup`, {
        method: 'POST',
        body: JSON.stringify({ amount }),
    });
}

/**
 * Withdraw funds from wallet
 * @param {number} userId 
 * @param {number} amount 
 * @returns {Promise<{id: string, userId: number, balance: number, createdAt: string, updatedAt: string}>}
 */
export async function withdrawFromWallet(userId, amount) {
    return apiFetch(`/wallet/user/${userId}/withdraw`, {
        method: 'POST',
        body: JSON.stringify({ amount }),
    });
}

// ============================================
// DONATION API
// ============================================

/**
 * Get all donations made by a user
 * @param {number} userId 
 * @returns {Promise<Array<{id: string, donorUserId: number, recipientUserId: number, amount: number, message: string, createdAt: string}>>}
 */
export async function getDonationsByDonor(userId) {
    return apiFetch(`/donation/donor/${userId}`);
}

/**
 * Get all donations received by a user
 * @param {number} userId 
 * @returns {Promise<Array<{id: string, donorUserId: number, recipientUserId: number, amount: number, message: string, createdAt: string}>>}
 */
export async function getDonationsByRecipient(userId) {
    return apiFetch(`/donation/recipient/${userId}`);
}

/**
 * Get all donations for a user (both sent and received)
 * @param {number} userId 
 * @returns {Promise<{sent: Array, received: Array, all: Array}>}
 */
export async function getAllDonations(userId) {
    const [sent, received] = await Promise.all([
        getDonationsByDonor(userId),
        getDonationsByRecipient(userId),
    ]);

    // Combine and sort by date
    const all = [...sent, ...received].sort(
        (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
    );

    return { sent, received, all };
}

/**
 * Create a new donation
 * @param {Object} donation
 * @param {number} donation.donorUserId
 * @param {number} donation.recipientUserId
 * @param {number} donation.amount
 * @param {string} [donation.message]
 * @returns {Promise<{id: string, donorUserId: number, recipientUserId: number, amount: number, message: string, createdAt: string}>}
 */
export async function createDonation({ donorUserId, recipientUserId, amount, message }) {
    return apiFetch('/donation', {
        method: 'POST',
        body: JSON.stringify({
            donorUserId,
            recipientUserId,
            amount,
            message: message || null,
        }),
    });
}

// ============================================
// HELPER FUNCTIONS
// ============================================

/**
 * Transform API donation data to frontend format
 * @param {Object} apiDonation - Donation from API
 * @param {Object} usersMap - Map of userId to user object
 * @param {number} currentUserId - Current logged-in user's ID
 * @returns {Object} Frontend-formatted donation
 */
export function transformDonation(apiDonation, usersMap, currentUserId) {
    const donorUser = usersMap[apiDonation.donorUserId] || {
        id: apiDonation.donorUserId,
        name: `User ${apiDonation.donorUserId}`,
        handle: `user${apiDonation.donorUserId}`,
        profileImage: 'http://dergipark.org.tr/assets/app/images/buddy_sample.png',
    };

    const recipientUser = usersMap[apiDonation.recipientUserId] || {
        id: apiDonation.recipientUserId,
        name: `User ${apiDonation.recipientUserId}`,
        handle: `user${apiDonation.recipientUserId}`,
        profileImage: 'http://dergipark.org.tr/assets/app/images/buddy_sample.png',
    };

    return {
        id: apiDonation.id,
        donatorId: apiDonation.donorUserId,
        recipientId: apiDonation.recipientUserId,
        donator: donorUser,
        recipient: recipientUser,
        amount: apiDonation.amount,
        message: apiDonation.message || '',
        donatedAt: apiDonation.createdAt,
    };
}

/**
 * Create a users map from an array of users
 * @param {Array} users 
 * @returns {Object} Map of userId to user object
 */
export function createUsersMap(users) {
    return users.reduce((map, user) => {
        map[user.id] = user;
        return map;
    }, {});
}

export default {
    // Wallet
    getWallet,
    createWallet,
    getOrCreateWallet,
    topUpWallet,
    withdrawFromWallet,
    // Donations
    getDonationsByDonor,
    getDonationsByRecipient,
    getAllDonations,
    createDonation,
    // Helpers
    transformDonation,
    createUsersMap,
};

