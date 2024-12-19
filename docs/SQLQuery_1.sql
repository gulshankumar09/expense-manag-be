USE master


GO
DECLARE @ServerName NVARCHAR(128), 
        @DatabaseName NVARCHAR(128), 
        @ConnectionString NVARCHAR(MAX);

-- Get the server and database name
SET @ServerName = @@SERVERNAME;
SET @DatabaseName = DB_NAME();

-- Construct the connection string
SET @ConnectionString = 'Server=' + @ServerName + ';Database=' + @DatabaseName + ';Trusted_Connection=True;';

-- Output the connection string
SELECT @ConnectionString AS ConnectionString;
SELECT @ServerName AS ServerName;


GO

USE AuthServiceDB;


SELECT * FROM USERS

