USE DemoDB;

CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    PublicationYear INT NULL
);

INSERT INTO Books (Title, PublicationYear) VALUES
('Harry Potter and the Philosopher''s Stone', 1997),
('1984', 1949),
('The Hobbit', 1937);

SELECT * FROM Books;