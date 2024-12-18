USE [master]
GO
/****** Object:  Database [Glense]    Script Date: 2.12.2024. 00:00:44 ******/
CREATE DATABASE [Glense]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Glense', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\Glense.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Glense_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\Glense_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Glense] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Glense].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Glense] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Glense] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Glense] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Glense] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Glense] SET ARITHABORT OFF 
GO
ALTER DATABASE [Glense] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Glense] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Glense] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Glense] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Glense] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Glense] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Glense] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Glense] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Glense] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Glense] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Glense] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Glense] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Glense] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Glense] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Glense] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Glense] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Glense] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Glense] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Glense] SET  MULTI_USER 
GO
ALTER DATABASE [Glense] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Glense] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Glense] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Glense] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Glense] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Glense] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Glense] SET QUERY_STORE = ON
GO
ALTER DATABASE [Glense] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Glense]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[categoryID] [int] IDENTITY(1,1) NOT NULL,
	[categoryName] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[categoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentLikes]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentLikes](
	[commentID] [int] NOT NULL,
	[userID] [int] NOT NULL,
	[isLiked] [bit] NULL,
 CONSTRAINT [PK_COMMENTLIKES] PRIMARY KEY CLUSTERED 
(
	[commentID] ASC,
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comments]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comments](
	[commentID] [int] IDENTITY(1,1) NOT NULL,
	[videoID] [int] NOT NULL,
	[userID] [int] NOT NULL,
	[commentText] [nvarchar](max) NOT NULL,
	[createdAt] [datetime] NULL,
	[parentCommentID] [int] NULL,
	[commentLikeCount] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[commentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Conversations]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Conversations](
	[conversationID] [int] IDENTITY(1,1) NOT NULL,
	[user1ID] [int] NOT NULL,
	[user2ID] [int] NOT NULL,
	[createdAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[conversationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UC_Conversation] UNIQUE NONCLUSTERED 
(
	[user1ID] ASC,
	[user2ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Donations]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Donations](
	[donatorID] [int] NOT NULL,
	[recipientID] [int] NOT NULL,
	[amount] [int] NOT NULL,
	[donatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Donations] PRIMARY KEY CLUSTERED 
(
	[donatorID] ASC,
	[recipientID] ASC,
	[donatedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Messages]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[messageID] [int] IDENTITY(1,1) NOT NULL,
	[conversationID] [int] NOT NULL,
	[senderID] [int] NOT NULL,
	[messageText] [nvarchar](max) NOT NULL,
	[sentAt] [datetime] NULL,
	[isSeen] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[messageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Subscriptions]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscriptions](
	[subscriberID] [int] NOT NULL,
	[subscribedToID] [int] NOT NULL,
 CONSTRAINT [PK_SUBSCRIPTIONS] PRIMARY KEY CLUSTERED 
(
	[subscriberID] ASC,
	[subscribedToID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[userID] [int] IDENTITY(1,1) NOT NULL,
	[username] [varchar](50) NOT NULL,
	[passwordSHA256] [nvarchar](max) NOT NULL,
	[email] [varchar](50) NOT NULL,
	[profilePictureURL] [nvarchar](max) NULL,
	[account] [nvarchar](50) NOT NULL,
	[createdAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VideoLikes]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VideoLikes](
	[videoID] [int] NOT NULL,
	[userID] [int] NOT NULL,
	[isLiked] [bit] NULL,
 CONSTRAINT [PK_VIDEOLIKES] PRIMARY KEY CLUSTERED 
(
	[videoID] ASC,
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Videos]    Script Date: 2.12.2024. 00:00:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Videos](
	[videoID] [int] IDENTITY(1,1) NOT NULL,
	[uploaderID] [int] NOT NULL,
	[title] [nvarchar](255) NOT NULL,
	[videoURL] [nvarchar](2083) NOT NULL,
	[thumbnailURL] [nvarchar](2083) NULL,
	[uploadedAt] [datetime] NULL,
	[viewCount] [int] NULL,
	[likeCount] [int] NULL,
	[dislikeCount] [int] NULL,
	[description] [nvarchar](max) NULL,
	[categoryID] [int] NOT NULL,
 CONSTRAINT [PK__Videos__14B0F5960F0D8E62] PRIMARY KEY CLUSTERED 
(
	[videoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Category] ADD  DEFAULT ('NO_CATEGORY') FOR [categoryName]
GO
ALTER TABLE [dbo].[CommentLikes] ADD  DEFAULT ((0)) FOR [isLiked]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT (getdate()) FOR [createdAt]
GO
ALTER TABLE [dbo].[Comments] ADD  CONSTRAINT [DF_Comments_commentLikeCount]  DEFAULT ((0)) FOR [commentLikeCount]
GO
ALTER TABLE [dbo].[Conversations] ADD  DEFAULT (getdate()) FOR [createdAt]
GO
ALTER TABLE [dbo].[Donations] ADD  DEFAULT (getdate()) FOR [donatedAt]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (getdate()) FOR [sentAt]
GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_isSeen]  DEFAULT ((0)) FOR [isSeen]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_createdAt]  DEFAULT (getdate()) FOR [createdAt]
GO
ALTER TABLE [dbo].[VideoLikes] ADD  CONSTRAINT [DF__VideoLike__isLik__5FB337D6]  DEFAULT ((0)) FOR [isLiked]
GO
ALTER TABLE [dbo].[Videos] ADD  CONSTRAINT [DF__Videos__uploaded__5441852A]  DEFAULT (getdate()) FOR [uploadedAt]
GO
ALTER TABLE [dbo].[Videos] ADD  CONSTRAINT [DF__Videos__viewCoun__5535A963]  DEFAULT ((0)) FOR [viewCount]
GO
ALTER TABLE [dbo].[Videos] ADD  CONSTRAINT [DF__Videos__likeCoun__5629CD9C]  DEFAULT ((0)) FOR [likeCount]
GO
ALTER TABLE [dbo].[Videos] ADD  CONSTRAINT [DF__Videos__dislikeC__571DF1D5]  DEFAULT ((0)) FOR [dislikeCount]
GO
ALTER TABLE [dbo].[CommentLikes]  WITH CHECK ADD  CONSTRAINT [FK_COMMENTLIKES_COMMENTS] FOREIGN KEY([commentID])
REFERENCES [dbo].[Comments] ([commentID])
GO
ALTER TABLE [dbo].[CommentLikes] CHECK CONSTRAINT [FK_COMMENTLIKES_COMMENTS]
GO
ALTER TABLE [dbo].[CommentLikes]  WITH CHECK ADD  CONSTRAINT [FK_COMMENTLIKES_USERS] FOREIGN KEY([userID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[CommentLikes] CHECK CONSTRAINT [FK_COMMENTLIKES_USERS]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_COMMENTS_COMMENTS] FOREIGN KEY([parentCommentID])
REFERENCES [dbo].[Comments] ([commentID])
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_COMMENTS_COMMENTS]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_COMMENTS_USERS] FOREIGN KEY([userID])
REFERENCES [dbo].[Users] ([userID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_COMMENTS_USERS]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_COMMENTS_VIDEOS] FOREIGN KEY([videoID])
REFERENCES [dbo].[Videos] ([videoID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_COMMENTS_VIDEOS]
GO
ALTER TABLE [dbo].[Conversations]  WITH CHECK ADD  CONSTRAINT [FK_Conversations_User1] FOREIGN KEY([user1ID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Conversations] CHECK CONSTRAINT [FK_Conversations_User1]
GO
ALTER TABLE [dbo].[Conversations]  WITH CHECK ADD  CONSTRAINT [FK_Conversations_User2] FOREIGN KEY([user2ID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Conversations] CHECK CONSTRAINT [FK_Conversations_User2]
GO
ALTER TABLE [dbo].[Donations]  WITH CHECK ADD  CONSTRAINT [FK_Donations_Donator] FOREIGN KEY([donatorID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Donations] CHECK CONSTRAINT [FK_Donations_Donator]
GO
ALTER TABLE [dbo].[Donations]  WITH CHECK ADD  CONSTRAINT [FK_Donations_Recipient] FOREIGN KEY([recipientID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Donations] CHECK CONSTRAINT [FK_Donations_Recipient]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_MESSAGES_CONVERSATIONS] FOREIGN KEY([conversationID])
REFERENCES [dbo].[Conversations] ([conversationID])
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_MESSAGES_CONVERSATIONS]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_MESSAGES_SENDER] FOREIGN KEY([senderID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_MESSAGES_SENDER]
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD  CONSTRAINT [FK_SUBSCRIPTIONS_SUBSCRIBEDTO] FOREIGN KEY([subscribedToID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_SUBSCRIPTIONS_SUBSCRIBEDTO]
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD  CONSTRAINT [FK_SUBSCRIPTIONS_SUBSCRIBER] FOREIGN KEY([subscriberID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_SUBSCRIPTIONS_SUBSCRIBER]
GO
ALTER TABLE [dbo].[VideoLikes]  WITH CHECK ADD  CONSTRAINT [FK_VIDEOLIKES_USERS] FOREIGN KEY([userID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[VideoLikes] CHECK CONSTRAINT [FK_VIDEOLIKES_USERS]
GO
ALTER TABLE [dbo].[VideoLikes]  WITH CHECK ADD  CONSTRAINT [FK_VIDEOLIKES_VIDEOS] FOREIGN KEY([videoID])
REFERENCES [dbo].[Videos] ([videoID])
GO
ALTER TABLE [dbo].[VideoLikes] CHECK CONSTRAINT [FK_VIDEOLIKES_VIDEOS]
GO
ALTER TABLE [dbo].[Videos]  WITH CHECK ADD  CONSTRAINT [FK_VIDEOS_CATEGORY] FOREIGN KEY([categoryID])
REFERENCES [dbo].[Category] ([categoryID])
GO
ALTER TABLE [dbo].[Videos] CHECK CONSTRAINT [FK_VIDEOS_CATEGORY]
GO
ALTER TABLE [dbo].[Videos]  WITH CHECK ADD  CONSTRAINT [FK_VIDEOS_USERS] FOREIGN KEY([uploaderID])
REFERENCES [dbo].[Users] ([userID])
GO
ALTER TABLE [dbo].[Videos] CHECK CONSTRAINT [FK_VIDEOS_USERS]
GO
USE [master]
GO
ALTER DATABASE [Glense] SET  READ_WRITE 
GO
