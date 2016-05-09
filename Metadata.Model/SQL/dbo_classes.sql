USE [zhichkin]
GO

/****** Object:  Table [dbo].[classes]    Script Date: 14.07.2015 16:49:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[classes](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[namespace] [uniqueidentifier] NOT NULL,
	[owner] [uniqueidentifier] NOT NULL,
	[parent] [uniqueidentifier] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[table] [nvarchar](50) NOT NULL,
	[code] [int] NOT NULL,
 CONSTRAINT [PK_classes] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

