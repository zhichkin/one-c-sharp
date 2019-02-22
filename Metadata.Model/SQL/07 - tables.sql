USE [Z]
GO

CREATE TABLE [metadata].[tables](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[schema] [nvarchar](100) NOT NULL,
	[entity] [uniqueidentifier] NOT NULL,
	[purpose] [int] NOT NULL,
 CONSTRAINT [PK_tables] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

