CREATE DATABASE TechStore;


CREATE TABLE Users (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Phone VARCHAR(20),
    UserType VARCHAR(50) NOT NULL CHECK (UserType IN ('Admin', 'Customer')),
    CreatedAt DATETIME DEFAULT GETDATE() 
);


CREATE TABLE Laptops (
    LaptopID INT PRIMARY KEY identity,
    Brand VARCHAR(100) NOT NULL,
    Model VARCHAR(255) NOT NULL,
    Processor VARCHAR(100) NOT NULL,
    RAM VARCHAR(50) NOT NULL,
    Storage VARCHAR(50) NOT NULL,
    GPU VARCHAR(100),
    ScreenSize DECIMAL(4,1),
    Price DECIMAL(10,2) NOT NULL,
    ImageURL VARCHAR(255),
    Description TEXT,
    Stock INT DEFAULT 0
);


CREATE TABLE PCs (
    PCID INT PRIMARY KEY identity,
    Brand VARCHAR(100) NOT NULL,
    Processor VARCHAR(100) NOT NULL,
    RAM VARCHAR(50) NOT NULL,
    Storage VARCHAR(50) NOT NULL,
    GPU VARCHAR(100),
    Price DECIMAL(10,2) NOT NULL,
    ImageURL VARCHAR(255),
    Description TEXT,
    Stock INT DEFAULT 0
);



CREATE TABLE PCParts (
    PartID INT PRIMARY KEY IDENTITY(1,1),
    Category VARCHAR(50) NOT NULL CHECK (Category IN ('RAM', 'GPU', 'SSD', 'HDD', 'Motherboard', 'Power Supply', 'Cooling System')),
    Brand VARCHAR(100) NOT NULL,
    Model VARCHAR(255) NOT NULL,
    Compatibility TEXT,
    Price DECIMAL(10,2) NOT NULL,
    ImageURL VARCHAR(255),
    Stock INT DEFAULT 0
);



CREATE TABLE Questions (
    QuestionID INT PRIMARY KEY identity,
    QuestionText TEXT NOT NULL,
    Option1 VARCHAR(255) NOT NULL,
    Option2 VARCHAR(255) NOT NULL,
    Option3 VARCHAR(255),
    Option4 VARCHAR(255),
    CorrectOption VARCHAR(255) 
);


CREATE TABLE UserAnswers (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    QuestionID INT,
    SelectedOption VARCHAR(255),
    Timestamp DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    FOREIGN KEY (QuestionID) REFERENCES Questions(QuestionID) ON DELETE CASCADE
);


CREATE TABLE Recommendations (
    RecommendationID INT PRIMARY KEY identity,
    UserID INT,
    LaptopID INT NULL,
    PCID INT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    FOREIGN KEY (LaptopID) REFERENCES Laptops(LaptopID) ON DELETE SET NULL,
    FOREIGN KEY (PCID) REFERENCES PCs(PCID) ON DELETE SET NULL
);



CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    TotalPrice DECIMAL(10,2) NOT NULL,
    OrderStatus VARCHAR(20) NOT NULL DEFAULT 'Pending', 
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    CHECK (OrderStatus IN ('Pending', 'Completed', 'Cancelled')) 
);





