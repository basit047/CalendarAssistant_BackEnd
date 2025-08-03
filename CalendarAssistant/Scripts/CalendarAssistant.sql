SelecT * from aspnetusers

--MeetingId,
--Eventid 26
--StatusId
--Attendees,
--Descriptions
--CreatedBy
--CreatedAt
--ModifiedBy
--ModifedAt

--drop table meetings

CREATE TABLE [dbo].[Meetings](
	[MeetingId] [int] IDENTITY(1,1) NOT NULL,
	[EventId] nvarchar(40) NOT NULL,
	[Attendees] nvarchar(300) NULL,
	[Description] nvarchar(100) NULL,
	[StartDateTime] datetime not null,
	[EndDateTime] datetime not null,
	[StatusId] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MeetingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[Meetings]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[AspNetUsers] ([UserId])
GO

ALTER TABLE [dbo].[Meetings]  WITH CHECK ADD FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[AspNetUsers] ([UserId])
GO




Alter table aspnetusers
ADD UNIQUE (UserId)




--25-04-2025


CREATE TABLE WorkingHour (
    Id [int] IDENTITY(1,1) NOT NULL,
    UserId int not null,
	DayId int not null,
	StartTime TIME,
    EndTime TIME,
    IsWorkingDay bit DEFAULT 1
);

ALTER TABLE [dbo].[WorkingHour]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([UserId])
GO

ALTER TABLE [dbo].[WorkingHour]  WITH CHECK ADD FOREIGN KEY([DayId])
REFERENCES [dbo].[DayWeek] ([Id])
GO



CREATE TABLE DayWeek
(
Id INT primary key NOT NULL,
[Day] [nvarchar](20) NOT NULL,
)


--DayWeek
INSERT INTO DayWeek values(0,'Sunday')
INSERT INTO DayWeek values(1,'Monday')
INSERT INTO DayWeek values(2,'Tuesday')
INSERT INTO DayWeek values(3,'Wednesday')
INSERT INTO DayWeek values(4,'Thursday')
INSERT INTO DayWeek values(5,'Friday')
INSERT INTO DayWeek values(6,'Saturday')


--WorkingHour
Insert into WorkingHour values(1, 0, '08:30', '17:30', 0) --Sunday
Insert into WorkingHour values(1, 1, '08:30', '17:30', 1) --Monday
Insert into WorkingHour values(1, 2, '08:30', '17:30', 0) --Tuesday
Insert into WorkingHour values(1, 3, '08:30', '17:30', 1) --Wednesday
Insert into WorkingHour values(1, 4, '08:30', '17:30', 1) --Thursday
Insert into WorkingHour values(1, 5, '08:30', '17:30', 0) --Friday
Insert into WorkingHour values(1, 6, '08:30', '17:30', 0) --Saturday



--03-05-2025

Create Table PollingSync
(
Id [int] IDENTITY(1,1) NOT NULL,
SyncDateTime DateTime not null,
UserId int not null,
[CreatedAt] [datetime] NOT NULL,
[ModifiedAt] [datetime] NULL,
)

ALTER TABLE [dbo].[PollingSync]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([UserId])
GO