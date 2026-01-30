-- Video Catalogue Microservice Schema
-- Note: User IDs reference the Account microservice (cross-service reference by ID only)

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Videos table
CREATE TABLE IF NOT EXISTS Videos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    thumbnail_url VARCHAR(512),
    video_url VARCHAR(512) NOT NULL,
    description TEXT,
    upload_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    uploader_id INT NOT NULL,  -- References user in Account microservice
    view_count INT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0
);

-- Playlists table
CREATE TABLE IF NOT EXISTS Playlists (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    creator_id INT NOT NULL,  -- References user in Account microservice
    creation_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- PlaylistVideos junction table
CREATE TABLE IF NOT EXISTS PlaylistVideos (
    playlist_id UUID NOT NULL,
    video_id UUID NOT NULL,
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (playlist_id, video_id),
    FOREIGN KEY (playlist_id) REFERENCES Playlists(id) ON DELETE CASCADE,
    FOREIGN KEY (video_id) REFERENCES Videos(id) ON DELETE CASCADE
);

-- Subscriptions table (user-to-user subscriptions)
CREATE TABLE IF NOT EXISTS Subscriptions (
    subscriber_id INT NOT NULL,      -- References user in Account microservice
    subscribed_to_id INT NOT NULL,   -- References user in Account microservice
    subscription_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (subscriber_id, subscribed_to_id)
);

-- VideoLikes table
CREATE TABLE IF NOT EXISTS VideoLikes (
    user_id INT NOT NULL,  -- References user in Account microservice
    video_id UUID NOT NULL,
    is_liked BOOLEAN NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, video_id),
    FOREIGN KEY (video_id) REFERENCES Videos(id) ON DELETE CASCADE
);

-- Indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_videos_uploader ON Videos(uploader_id);
CREATE INDEX IF NOT EXISTS idx_videos_upload_date ON Videos(upload_date DESC);
CREATE INDEX IF NOT EXISTS idx_playlists_creator ON Playlists(creator_id);
CREATE INDEX IF NOT EXISTS idx_subscriptions_subscriber ON Subscriptions(subscriber_id);
CREATE INDEX IF NOT EXISTS idx_subscriptions_subscribed_to ON Subscriptions(subscribed_to_id);
CREATE INDEX IF NOT EXISTS idx_video_likes_video ON VideoLikes(video_id);
