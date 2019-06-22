USE [Z]
GO

CREATE TABLE [metadata].[properties](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[entity] [uniqueidentifier] NOT NULL,
	[purpose] [int] NOT NULL,
	[ordinal] [int] NOT NULL,
	[is_abstract] [bit] NOT NULL,
	[is_read_only] [bit] NOT NULL,
	[is_primary_key] [bit] NOT NULL,
 CONSTRAINT [PK_properties] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

