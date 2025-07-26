USE Glense;

INSERT INTO [dbo].[Users] (username, passwordSHA256, email, profilePictureURL, account, createdAt)
VALUES
    ('branko', 'hashed_password_1', 'brankogrbic@email.com', NULL, 'admin', GETDATE()),
    ('john_doe', 'hashed_password_2', '', 'https://example.com/profile/john_doe.jpg', 'user', GETDATE());