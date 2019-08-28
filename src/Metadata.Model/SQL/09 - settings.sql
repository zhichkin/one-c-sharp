USE [one-c-sharp];
GO

CREATE TABLE [metadata].[settings](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[owner] [uniqueidentifier] NOT NULL,
	[owner_] [int] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[value] [nvarchar](1024) NOT NULL,
 CONSTRAINT [PK_settings] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

