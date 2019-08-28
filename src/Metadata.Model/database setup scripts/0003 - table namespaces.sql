USE [one-c-sharp];

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = N'namespaces' AND type = N'U')
BEGIN
	CREATE TABLE [metadata].[namespaces](
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
END

IF NOT EXISTS(SELECT 1 FROM [metadata].[namespaces]
			   WHERE [key]    = CAST(0x00000000000000000000000000000000 AS uniqueidentifier)
			     AND [owner_] = 1
				 AND [owner]  = CAST(0x00000000000000000000000000000000 AS uniqueidentifier))
BEGIN
	INSERT [metadata].[namespaces] ([key],[owner_],[owner],[name])
	VALUES
	(
		CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
		1, -- InfoBase
		CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
		N'TypeSystem'
	);
END