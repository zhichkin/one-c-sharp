USE [zhichkin]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[namespaces](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[owner] [uniqueidentifier] NOT NULL,
	[owner_] [int] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_namespaces] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT [dbo].[namespaces]
(
	[key],
	[owner_],
	[owner],
	[name]
)
VALUES
(
	CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
	1, -- InfoBase
	CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
	N'TypeSystem'
);
GO

