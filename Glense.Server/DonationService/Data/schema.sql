-- Donation Microservice Database Schema for Neon (PostgreSQL)
-- Run this script in your Neon database to create the tables

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Wallets table: stores balance for each user
CREATE TABLE IF NOT EXISTS wallets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id INT NOT NULL UNIQUE,  -- Reference to Account microservice
    balance DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Donations table: records all donation transactions
CREATE TABLE IF NOT EXISTS donations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    donor_user_id INT NOT NULL,
    recipient_user_id INT NOT NULL,
    amount DECIMAL(12, 2) NOT NULL CHECK (amount > 0),
    message TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Ensure donor and recipient are different
    CONSTRAINT different_users CHECK (donor_user_id != recipient_user_id)
);

-- Indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_wallets_user_id ON wallets(user_id);
CREATE INDEX IF NOT EXISTS idx_donations_donor ON donations(donor_user_id);
CREATE INDEX IF NOT EXISTS idx_donations_recipient ON donations(recipient_user_id);
CREATE INDEX IF NOT EXISTS idx_donations_created_at ON donations(created_at DESC);

-- Function to update the updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger to auto-update updated_at on wallets
DROP TRIGGER IF EXISTS update_wallets_updated_at ON wallets;
CREATE TRIGGER update_wallets_updated_at
    BEFORE UPDATE ON wallets
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Insert some test data (optional - for development)
-- INSERT INTO wallets (user_id, balance) VALUES 
--     (1, 1250.75),
--     (2, 500.00),
--     (3, 750.50),
--     (4, 2000.00),
--     (5, 100.00),
--     (6, 300.25);

