USE [zhichkin]
GO

/****** Object:  Table [dbo].[properties_fields]    Script Date: 17.07.2015 10:44:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[properties_fields](
	[field] [uniqueidentifier] NOT NULL,
	[property] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_properties_fields] PRIMARY KEY CLUSTERED 
(
	[field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

