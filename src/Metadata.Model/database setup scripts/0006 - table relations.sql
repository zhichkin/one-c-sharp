USE [one-c-sharp];

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = N'relations' AND type = N'U')
BEGIN
	CREATE TABLE [metadata].[relations](
		[property] [uniqueidentifier] NOT NULL,
		[entity] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_relations] PRIMARY KEY CLUSTERED 
		(
			[property] ASC,
			[entity] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END