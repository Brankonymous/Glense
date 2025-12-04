USE Glense;

-- Disable constraints
GO
ALTER TABLE Messages NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Conversations NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Subscriptions NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Donations NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE CommentLikes NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Comments NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE VideoLikes NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Videos NOCHECK CONSTRAINT ALL;
GO
ALTER TABLE Users NOCHECK CONSTRAINT ALL;

-- Delete data in the correct order
GO
DELETE FROM Messages;
GO
DELETE FROM Conversations;
GO
DELETE FROM Subscriptions;
GO
DELETE FROM Donations;
GO
DELETE FROM CommentLikes;
GO
DELETE FROM Comments;
GO
DELETE FROM VideoLikes;
GO
DELETE FROM Videos;
GO
DELETE FROM Users;
GO
DELETE FROM Category;

-- Reset identity counters
GO
DBCC CHECKIDENT ('Users', RESEED, 1);
GO
DBCC CHECKIDENT ('Videos', RESEED, 1);
GO
DBCC CHECKIDENT ('Comments', RESEED, 1);
GO
DBCC CHECKIDENT ('Category', RESEED, 1);
GO
DBCC CHECKIDENT ('Conversations', RESEED, 1);
GO
DBCC CHECKIDENT ('Messages', RESEED, 1);

-- Re-enable constraints
GO
ALTER TABLE Messages CHECK CONSTRAINT ALL;
GO
ALTER TABLE Conversations CHECK CONSTRAINT ALL;
GO
ALTER TABLE Subscriptions CHECK CONSTRAINT ALL;
GO
ALTER TABLE Donations CHECK CONSTRAINT ALL;
GO
ALTER TABLE CommentLikes CHECK CONSTRAINT ALL;
GO
ALTER TABLE Comments CHECK CONSTRAINT ALL;
GO
ALTER TABLE VideoLikes CHECK CONSTRAINT ALL;
GO
ALTER TABLE Videos CHECK CONSTRAINT ALL;
GO
ALTER TABLE Users CHECK CONSTRAINT ALL;

GO
INSERT INTO Users (username, passwordSHA256, email, profilePictureURL, account, createdAt) VALUES
	('alice', 'ef92b778bafe771e89245b89ecbcf8a8b2406e1a7f7a7a7a7a7a7a7a7a7a7a7a', 'alice@mail.com', 'http://example.com/alice.jpg', 'Premium', GETDATE()),
	('bob', '5e884898da28047151d0e56f8dc6292773603d0d6aabbddc8a7a7a7a7a7a7a7a', 'bob@mail.com', 'http://example.com/bob.jpg', 'Free', GETDATE()),
	('charlie', '6b3a55e0261b0304143f805a249d6a7a7a7a7a7a7a7a7a7a7a7a7a7a7a7a7a7a7a7a', 'charlie@mail.com', 'http://example.com/charlie.jpg', 'Premium', GETDATE());
GO
INSERT INTO Category (categoryName) VALUES
	('Music'),
	('Gaming'),
	('Education');

GO
INSERT INTO Videos (uploaderID, title, videoURL, thumbnailURL, uploadedAt, viewCount, likeCount, dislikeCount, description, categoryID) VALUES
	(1, 'Wildlife Sample Video', 'https://www.youtube.com/watch?v=a3ICNMQW7Ok', 'https://picsum.photos/id/237/200/300', '2024-10-10', 1000, 150, 10, 'Wildlife', 1),
	(2, 'Background Sample Video', 'https://www.youtube.com/watch?v=K4TOrB7at0Y', 'https://picsum.photos/seed/picsum/200/300', '2024-02-05', 2000, 300, 20, 'Dont know', 2);

GO
INSERT INTO Comments (videoID, userID, commentText, createdAt, parentCommentID, commentLikeCount) VALUES
	(1, 2, 'Great video!', GETDATE(), NULL, 5),
	(1, 3, 'Thanks for the video!', GETDATE(), NULL, 2),
	(1, 1, 'You''re welcome!', GETDATE(), 1, 1);

GO
INSERT INTO VideoLikes (videoID, userID, isLiked) VALUES
	(1, 2, 1),
	(1, 3, 1),
	(2, 1, 0);

GO
INSERT INTO CommentLikes (commentID, userID, isLiked) VALUES
	(1, 1, 1),
	(2, 1, 1),
	(3, 2, 0);

GO
INSERT INTO Donations (donatorID, recipientID, amount, donatedAt) VALUES
	(2, 1, 25.00, GETDATE()),
	(3, 1, 10.00, GETDATE());

GO
INSERT INTO Subscriptions (subscriberID, subscribedToID) VALUES
	(2, 1),
	(3, 1),
	(1, 2);

GO
INSERT INTO Conversations (user1ID, user2ID, createdAt) VALUES
	(1, 2, GETDATE()),
	(1, 3, GETDATE());

GO
INSERT INTO Messages (conversationID, senderID, messageText, sentAt, isSeen) VALUES
	(1, 1, 'Hey Bob!', GETDATE(), 0),
	(1, 2, 'Hi Alice!', GETDATE(), 1),
	(2, 1, 'Hi Charlie!', GETDATE(), 1);
