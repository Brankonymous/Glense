import MusicNoteIcon from "@mui/icons-material/MusicNote";
import HomeIcon from "@mui/icons-material/Home";
import OndemandVideoIcon from "@mui/icons-material/OndemandVideo";
import SportsEsportsIcon from "@mui/icons-material/SportsEsports";
import LiveTvIcon from "@mui/icons-material/LiveTv";
import SchoolIcon from "@mui/icons-material/School";
import FaceRetouchingNaturalIcon from "@mui/icons-material/FaceRetouchingNatural";
import CheckroomIcon from "@mui/icons-material/Checkroom";
import GraphicEqIcon from "@mui/icons-material/GraphicEq";
import TheaterComedyIcon from "@mui/icons-material/TheaterComedy";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import DeveloperModeIcon from "@mui/icons-material/DeveloperMode";

export const categories = [
    { name: "New Videos", icon: <HomeIcon /> },
    { name: "Music", icon: <MusicNoteIcon /> },
    { name: "Education", icon: <SchoolIcon /> },
    { name: "Podcast", icon: <GraphicEqIcon /> },
    { name: "Movie", icon: <OndemandVideoIcon /> },
    { name: "Gaming", icon: <SportsEsportsIcon /> },
    { name: "Live", icon: <LiveTvIcon /> },
    { name: "Sport", icon: <FitnessCenterIcon /> },
    { name: "Fashion", icon: <CheckroomIcon /> },
    { name: "Beauty", icon: <FaceRetouchingNaturalIcon /> },
    { name: "Comedy", icon: <TheaterComedyIcon /> },
    { name: "Gym", icon: <FitnessCenterIcon /> },
    { name: "Crypto", icon: <DeveloperModeIcon /> },
];

export const chats = [
    { 
        name: "Keki", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png",
        messages: [
            {sender: "Keki", message: "Hello", time: "12:00", isMe: false},
            {sender: "Branko", message: "Hello", time: "12:05", isMe: true},
            {sender: "Keki", message: "How are you", time: "12:10", isMe: false},
            {sender: "Keki", message: "Ok.", time: "12:10", isMe: false},
        ]
    },
    { 
        name: "Irena", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png",
        messages: [
            {sender: "Irena", message: "Smorena sam nesto danas", time: "12:00", isMe: false},
            {sender: "Branko", message: "Briga mee", time: "12:05", isMe: true},
            {sender: "Irena", message: ":(", time: "12:10", isMe: false},
            {sender: "Irena", message: "Sta radis", time: "13:10", isMe: false},
        ]
    }
];

export const videos = [];
for (let i = 0; i < 100; i++) {
    videos.push({
        id: { videoId: "haDjmBT9tu4" },
        title: "An Honest Review of Apple Intelligence\... So Far",
        url: "https://www.youtube.com/watch?v=haDjmBT9tu4"
    });
}

export const demoThumbnailUrl = "https://i.ibb.co/G2L2Gwp/API-Course.png";
export const demoChannelUrl = "/channel/UCmXmlB4-HJytD7wek0Uo97A";
export const demoVideoUrl = "/video/GDa8kZLNhJ4";
export const demoChannelTitle = "JavaScript Mastery";
export const demoVideoTitle =
    "Build and Deploy 5 JavaScript & React API Projects in 10 Hours - Full Course | RapidAPI";
export const demoProfilePicture =
    "http://dergipark.org.tr/assets/app/images/buddy_sample.png";

// Users for donations
export const users = [
    { 
        id: 1, 
        name: "Branko Grbic", 
        handle: "branko", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
    { 
        id: 2, 
        name: "Keki Master", 
        handle: "keki", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
    { 
        id: 3, 
        name: "Irena Sweet", 
        handle: "irena", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
    { 
        id: 4, 
        name: "Marques Brownlee", 
        handle: "mkbhd", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
    { 
        id: 5, 
        name: "Linus Tech Tips", 
        handle: "linustech", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
    { 
        id: 6, 
        name: "Casey Neistat", 
        handle: "casey", 
        profileImage: "http://dergipark.org.tr/assets/app/images/buddy_sample.png" 
    },
];

export const recentRecipients = [
    users[1], // Keki
    users[2], // Irena  
    users[3], // MKBHD
];

// Donation history
export const donationHistory = [
    {
        donatorId: 1,
        recipientId: 2,
        donator: users[0],
        recipient: users[1],
        amount: 25,
        message: "Great content, keep it up!",
        donatedAt: "2024-12-09T14:30:00Z"
    },
    {
        donatorId: 3,
        recipientId: 1,
        donator: users[2],
        recipient: users[0],
        amount: 50,
        message: "Thanks for the help!",
        donatedAt: "2024-12-08T10:15:00Z"
    },
    {
        donatorId: 1,
        recipientId: 4,
        donator: users[0],
        recipient: users[3],
        amount: 100,
        message: "Love your tech reviews!",
        donatedAt: "2024-12-07T18:45:00Z"
    },
    {
        donatorId: 4,
        recipientId: 1,
        donator: users[3],
        recipient: users[0],
        amount: 10,
        message: "",
        donatedAt: "2024-12-06T09:20:00Z"
    },
    {
        donatorId: 1,
        recipientId: 2,
        donator: users[0],
        recipient: users[1],
        amount: 15,
        message: "Happy birthday!",
        donatedAt: "2024-12-05T12:00:00Z"
    },
    {
        donatorId: 5,
        recipientId: 1,
        donator: users[4],
        recipient: users[0],
        amount: 75,
        message: "Awesome work on the project!",
        donatedAt: "2024-12-04T16:30:00Z"
    },
];

// Comment
export const comments = [];
for (let i = 0; i < 50; i++) {
    comments.push({
        id: i,
        name: "John Doe",
        imageUrl: "https://i.ibb.co/G2L2Gwp/API-Course.png",
        commentText: "This is a comment",
        likeCount: 10,
    });
}


// VideoStream
export const videoInfo = {
    publishedAt: 'Nov 22, 2024',
    channelId: 'mkbhd',
    title: 'An Honest Review of Apple Intelligence... So Far',
    description: 'Reviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\n',
    channelTitle: 'Marques Brownlee',
    tags: ['Apple'],
    viewCount: 2364175,
    likeCount: 123456,
    dislikeCount: 1234
};