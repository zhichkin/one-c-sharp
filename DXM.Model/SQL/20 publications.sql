USE [Z];
GO

CREATE TABLE [dxm].[publications](
	[key]       [uniqueidentifier] NOT NULL,
	[version]   [rowversion]       NOT NULL,
	[name]      [nvarchar](128)    NOT NULL,
	[publisher] [uniqueidentifier] NOT NULL, -- foreign key to InfoBase
 CONSTRAINT [pk_publications] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO