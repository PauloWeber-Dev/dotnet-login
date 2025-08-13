-- Declare variables
DECLARE @DatabaseName NVARCHAR(128) = 'login_net';
DECLARE @LoginName NVARCHAR(128) = 'login_user';
DECLARE @Password NVARCHAR(128) = '123456'; -- Replace
DECLARE @SQL NVARCHAR(MAX);

-- Check if the database exists, create if it doesn't
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @DatabaseName)
BEGIN
    BEGIN TRY
        SET @SQL = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
        EXEC sp_executesql @SQL;
        PRINT 'Database ' + @DatabaseName + ' created successfully.';
    END TRY
    BEGIN CATCH
        PRINT 'Error creating database: ' + ERROR_MESSAGE();
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT 'Database ' + @DatabaseName + ' already exists.';
END

-- Check if the login exists, create if it doesn't
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    BEGIN TRY
        SET @SQL = N'CREATE LOGIN ' + QUOTENAME(@LoginName) + 
                   N' WITH PASSWORD = ''' + @Password + '''';
        EXEC sp_executesql @SQL;
        PRINT 'Login ' + @LoginName + ' created successfully.';
        
        -- Optionally, add the login to the database with a role
        SET @SQL = N'USE ' + QUOTENAME(@DatabaseName) + N'; ' +
                   N'CREATE USER ' + QUOTENAME(@LoginName) + 
                   N' FOR LOGIN ' + QUOTENAME(@LoginName) + N'; ' +
                   N'ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@LoginName) + N'; ' +
                   N'ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@LoginName) + N';';
        EXEC sp_executesql @SQL;
        PRINT 'User ' + @LoginName + ' added to database with read/write permissions.';
    END TRY
    BEGIN CATCH
        PRINT 'Error creating login or user: ' + ERROR_MESSAGE();
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT 'Login ' + @LoginName + ' already exists.';
END
USE [login_net]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 13/08/2025 16:56:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[BirthDate] [date] NOT NULL,
	[Gender] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[PasswordHash] [nvarchar](255) NULL,
	[GoogleId] [nvarchar](255) NULL,
	[ResetToken] [nvarchar](255) NULL,
	[ResetTokenExpiry] [datetime] NULL,
	[MfaSecretKey] [nvarchar](255) NULL,
	[EmailConfirmationCode] [nvarchar](255) NULL,
	[EmailConfirmationCodeExpiry] [datetime] NULL,
	[FacebookId] [nvarchar](255) NULL,
	[Roles] [nvarchar](255) NULL,
	[Status] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [Status]
GO
