
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/23/2013 17:56:05
-- Generated from EDMX file: C:\Users\injac\documents\visual studio 2013\Projects\ExGripAuthServer\AuthService\Model\UserApps.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [userapps];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[apps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[apps];
GO
IF OBJECT_ID(N'[dbo].[appusers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[appusers];
GO
IF OBJECT_ID(N'[dbo].[users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'apps'
CREATE TABLE [dbo].[apps] (
    [idapps] int IDENTITY(1,1) NOT NULL,
    [systemuserid] int  NOT NULL,
    [appname] nvarchar(120)  NOT NULL
);
GO

-- Creating table 'appusers'
CREATE TABLE [dbo].[appusers] (
    [idappusers] int IDENTITY(1,1) NOT NULL,
    [appid] int  NOT NULL,
    [appuserid] int  NOT NULL,
    [appSecret] nvarchar(65)  NOT NULL,
    [apptoken] nvarchar(65)  NOT NULL,
    [securitySoup] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'users'
CREATE TABLE [dbo].[users] (
    [iduser] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(45)  NOT NULL,
    [password] nvarchar(120)  NOT NULL,
    [firstname] nvarchar(45)  NULL,
    [lastname] nvarchar(45)  NULL,
    [address1] nvarchar(45)  NULL,
    [address2] nvarchar(45)  NULL,
    [country] nvarchar(45)  NULL,
    [zipcode] nvarchar(30)  NULL,
    [state] nvarchar(45)  NULL,
    [email] nvarchar(45)  NULL,
    [phone] nvarchar(45)  NULL,
    [fax] nvarchar(45)  NULL,
    [website] nvarchar(120)  NULL,
    [twitter] nvarchar(200)  NULL,
    [facebook] nvarchar(200)  NULL,
    [linkedin] nvarchar(200)  NULL,
    [age] int  NULL,
    [birthday] datetime  NULL,
    [city] nvarchar(45)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [idapps], [systemuserid] in table 'apps'
ALTER TABLE [dbo].[apps]
ADD CONSTRAINT [PK_apps]
    PRIMARY KEY CLUSTERED ([idapps], [systemuserid] ASC);
GO

-- Creating primary key on [idappusers], [appid], [appuserid], [appSecret], [apptoken] in table 'appusers'
ALTER TABLE [dbo].[appusers]
ADD CONSTRAINT [PK_appusers]
    PRIMARY KEY CLUSTERED ([idappusers], [appid], [appuserid], [appSecret], [apptoken] ASC);
GO

-- Creating primary key on [iduser] in table 'users'
ALTER TABLE [dbo].[users]
ADD CONSTRAINT [PK_users]
    PRIMARY KEY CLUSTERED ([iduser] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------