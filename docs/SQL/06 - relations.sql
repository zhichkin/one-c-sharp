USE [Z]
GO

CREATE TABLE [metadata].[relations](
	[property] [uniqueidentifier] NOT NULL,
	[entity] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_relations] PRIMARY KEY CLUSTERED 
(
	[property] ASC,
	[entity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

