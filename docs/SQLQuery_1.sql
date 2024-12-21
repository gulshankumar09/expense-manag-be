USE master


GO

--------------------------------------------------------------
-- To check the server name and make connection string via query
--------------------------------------------------------------
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

----------------------------------------------------------------------------------------------------
GO
----------------------------------------------------------------------------------------------------

SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';



DROP DATABASE AuthServiceDB;

----------------------------------------------------------------------------------------------------
GO
----------------------------------------------------------------------------------------------------

USE AuthServiceDB;

EXEC sp_help AsoNetUsers;


SELECT * FROM AspNetUsers



----------------------------------------------------------------------------------------------------
GO
----------------------------------------------------------------------------------------------------

-- Use this query to find tables referencing AspNetUsers through foreign keys:
SELECT 
    f.name AS ForeignKeyName,
    OBJECT_NAME(f.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName
FROM 
    sys.foreign_keys AS f
INNER JOIN 
    sys.foreign_key_columns AS fc
    ON f.object_id = fc.constraint_object_id
WHERE 
    OBJECT_NAME(f.referenced_object_id) = 'AspNetUsers';


-- ## 1. Delete data from the dependent tables before deleting from AspNetUsers. For instance:

    DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers);
    DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT Id FROM AspNetUsers);
    DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT Id FROM AspNetUsers);
    DELETE FROM AspNetUserTokens WHERE UserId IN (SELECT Id FROM AspNetUsers);

    -- Once all related data is removed, delete the users:

    DELETE FROM AspNetUsers;



-- ## 2. Alternative: Disable Foreign Key Constraints Temporarily

    --If there are many related tables, you can temporarily disable foreign key constraints:

    EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";


	-- Truncate the AspNetUsers table:

    TRUNCATE TABLE AspNetUsers;


	-- Re-enable constraints:

    EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
