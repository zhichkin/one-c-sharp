USE [zhichkin]
GO

/****** Object:  Table [dbo].[properties_classes]    Script Date: 16.07.2015 17:35:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[properties_classes](
	[property] [uniqueidentifier] NOT NULL,
	[class] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_properties_classes] PRIMARY KEY CLUSTERED 
(
	[property] ASC,
	[class] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

