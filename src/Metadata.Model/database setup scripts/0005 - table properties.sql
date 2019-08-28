USE [one-c-sharp];

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = N'properties' AND type = N'U')
BEGIN
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
END

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = N'ncx_properties_entity' AND object_id = OBJECT_ID(N'metadata.properties'))
BEGIN
	CREATE NONCLUSTERED INDEX ncx_properties_entity ON [metadata].[properties] ([entity]);
END
