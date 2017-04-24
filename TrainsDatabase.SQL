/*
Created: 2017-04-22
Modified: 2017-04-22
Model: Microsoft SQL Server 2014
Database: MS SQL Server 2014
*/


-- Create tables section -------------------------------------------------

-- Table Stops

CREATE TABLE [Stops]
(
 [stop_id] Nvarchar(10) NOT NULL,
 [name] Nvarchar(30) NULL,
 [street_id] Nvarchar(10) NULL,
 [lat] Nvarchar(10) NOT NULL,
 [lon] Nvarchar(10) NOT NULL,
 [direction] Nvarchar(30) NULL,
 [validity] Nvarchar(30) NULL
)
go

-- Add keys for table Stops

ALTER TABLE [Stops] ADD CONSTRAINT [Key1] PRIMARY KEY ([stop_id])
go

-- Table Lines

CREATE TABLE [Lines]
(
 [stop_id] Nvarchar(10) NOT NULL,
 [no_line] Nvarchar(10) NOT NULL
)
go

-- Add keys for table Lines

ALTER TABLE [Lines] ADD CONSTRAINT [Key2] PRIMARY KEY ([stop_id],[no_line])
go

-- Table Timetable

CREATE TABLE [Timetable]
(
 [stop_id] Nvarchar(10) NOT NULL,
 [no_line] Nvarchar(10) NOT NULL,
 [brigade] Nvarchar(10) NOT NULL,
 [direction] Nvarchar(30) NOT NULL,
 [route] Nvarchar(10) NOT NULL,
 [time] Nvarchar(20) NOT NULL
)
go

-- Add keys for table Timetable

ALTER TABLE [Timetable] ADD CONSTRAINT [Key3] PRIMARY KEY ([time],[stop_id],[no_line],[route],[brigade],[direction])
go

-- Table Trains

CREATE TABLE [Trains]
(
 [lat] Nvarchar(10) NOT NULL,
 [lines] Nvarchar(10) NOT NULL,
 [brigade] Nvarchar(10) NOT NULL,
 [lon] Nvarchar(10) NOT NULL,
 [time] Nvarchar(20) NOT NULL
)
go

-- Add keys for table Trains

ALTER TABLE [Trains] ADD CONSTRAINT [Key4] PRIMARY KEY ([lat],[lon],[lines],[brigade],[time])
go

-- Table TrainsHistory

CREATE TABLE [TrainsHistory]
(
 [lat] Nvarchar(10) NOT NULL,
 [lines] Nvarchar(10) NOT NULL,
 [brigade] Nvarchar(10) NOT NULL,
 [lon] Nvarchar(10) NOT NULL,
 [time] Nvarchar(20) NOT NULL
)
go

-- Table TrainsOld

CREATE TABLE [TrainsOld]
(
 [status] Nvarchar(10) NULL,
 [firstline] Nvarchar(10) NOT NULL,
 [lon] Nvarchar(10) NOT NULL,
 [lines] Nvarchar(20) NULL,
 [time] Nvarchar(10) NOT NULL,
 [lat] Nvarchar(10) NOT NULL,
 [lowfloor] Nvarchar(10) NULL,
 [brigade] Nvarchar(10) NOT NULL
)
go

-- Add keys for table TrainsOld

ALTER TABLE [TrainsOld] ADD CONSTRAINT [Key6] PRIMARY KEY ([lon],[lat],[time],[brigade],[firstline])
go



