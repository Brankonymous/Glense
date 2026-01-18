CREATE TABLE IF NOT EXISTS Videos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    thumbnail_url VARCHAR(512),
    video_url VARCHAR(512) NOT NULL,
    description TEXT,
    upload_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    uploader_id INT NOT NULL,
    view_count INT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    FOREIGN KEY (uploader_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS Playlists (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    creator_id INT NOT NULL,
    creation_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (creator_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS PlaylistVideos (
    playlist_id UUID NOT NULL,
    video_id UUID NOT NULL,
    PRIMARY KEY (playlist_id, video_id),
    FOREIGN KEY (playlist_id) REFERENCES Playlists(id),
    FOREIGN KEY (video_id) REFERENCES Videos(id)
);

CREATE TABLE IF NOT EXISTS Subscriptions (
    subscriber_id INT NOT NULL,
    subscribed_to_id INT NOT NULL,
    subscription_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (subscriber_id, subscribed_to_id),
    FOREIGN KEY (subscriber_id) REFERENCES Users(id),
    FOREIGN KEY (subscribed_to_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS VideoLikes(
    user_id INT NOT NULL,
    video_id UUID NOT NULL,
    is_liked BOOLEAN NOT NULL,
    PRIMARY KEY (user_id, video_id),
    FOREIGN KEY (user_id) REFERENCES Users(id),
    FOREIGN KEY (video_id) REFERENCES Videos(id)
);