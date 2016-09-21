USE [Z];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'metadata')
BEGIN
    -- The schema must be run in its own batch!
    EXECUTE('CREATE SCHEMA [metadata];');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'integrator')
BEGIN
    -- The schema must be run in its own batch!
    EXECUTE('CREATE SCHEMA [integrator];');
END
GO

CREATE TABLE [metadata].[infobases](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[server] [nvarchar](100) NOT NULL,
	[database] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_infobases] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT [metadata].[infobases] ([key], [name], [server], [database])
VALUES (CAST(0x00000000000000000000000000000000 AS uniqueidentifier), N'Metadata', N'', N'');
GO

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
GO

INSERT [metadata].[namespaces] ([key],[owner_],[owner],[name])
VALUES
(
	CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
	1, -- InfoBase
	CAST(0x00000000000000000000000000000000 AS uniqueidentifier),
	N'TypeSystem'
);
GO

CREATE TABLE [metadata].[entities](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[code] [int] NOT NULL,
	[namespace] [uniqueidentifier] NOT NULL,
	[owner] [uniqueidentifier] NOT NULL, -- nesting
	[parent] [uniqueidentifier] NOT NULL, -- inheritance
 CONSTRAINT [PK_entities] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT [metadata].[entities] ([key], [code], [name], [namespace], [owner], [parent])
VALUES
(0x00000000000000000000000000000000,   0, N'Empty',    0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFF,  -1, N'Object',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFE,  -2, N'DBNull',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFD,  -3, N'Boolean',  0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFC,  -4, N'Char',     0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFB,  -5, N'SByte',    0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFFA,  -6, N'Byte',     0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF9,  -7, N'Int16',    0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF8,  -8, N'UInt16',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF7,  -9, N'Int32',    0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF6, -10, N'UInt32',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF5, -11, N'Int64',    0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF4, -12, N'UInt64',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF3, -13, N'Single',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF2, -14, N'Double',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF1, -15, N'Decimal',  0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFF0, -16, N'DateTime', 0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFEF, -17, N'GUID',     0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFEE, -18, N'String',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFED, -19, N'Binary',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFEC, -20, N'List',     0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000),
(0x000000000000000000000000FFFFFFEB, -21, N'ObjRef',   0x00000000000000000000000000000000, 0x00000000000000000000000000000000, 0x00000000000000000000000000000000);

CREATE TABLE [metadata].[properties](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[entity] [uniqueidentifier] NOT NULL,
	[purpose] [int] NOT NULL,
 CONSTRAINT [PK_properties] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
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

CREATE TABLE [metadata].[fields] (
    [key]            UNIQUEIDENTIFIER NOT NULL,
    [version]        ROWVERSION       NOT NULL,
    [name]           NVARCHAR (100)   NOT NULL,
    [table]          UNIQUEIDENTIFIER NOT NULL,
	[property]       UNIQUEIDENTIFIER NOT NULL,
    [purpose]        INT              NOT NULL,
    [type_name]      NVARCHAR (16)    NOT NULL,
    [length]         INT              NOT NULL,
    [precision]      INT              NOT NULL,
    [scale]          INT              NOT NULL,
    [is_nullable]    BIT              NOT NULL,
	[is_primary_key] BIT              NOT NULL,
	[key_ordinal]    TINYINT          NOT NULL
	CONSTRAINT [PK_fields] PRIMARY KEY CLUSTERED 
	(
		[key] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [integrator].[publishers](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[last_sync_version] [bigint] NOT NULL,
	[msmq_target_queue] [nvarchar](256) NOT NULL,
 CONSTRAINT [pk_integrator_publishers] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [integrator].[subscriptions](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[publisher] [uniqueidentifier] NOT NULL,
	[subscriber] [uniqueidentifier] NOT NULL,
 CONSTRAINT [pk_integrator_subscriptions] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [integrator].[translation_rules](
	[source]          [uniqueidentifier] NOT NULL,
	[target]          [uniqueidentifier] NOT NULL,
	[source_property] [uniqueidentifier] NOT NULL,
	[target_property] [uniqueidentifier] NOT NULL,
	[is_sync_key]     [bit]              NOT NULL,
 CONSTRAINT [PK_translation_rules] PRIMARY KEY CLUSTERED 
(
	[source] ASC,
	[target] ASC,
	[source_property] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE PROCEDURE [integrator].[get_corresponding_source_entity]
	@source_infobase uniqueidentifier, 
	@target_infobase uniqueidentifier,
	@type_code int
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#source_namespaces') IS NOT NULL DROP TABLE #source_namespaces;
	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
	
    WITH source_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @source_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			source_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #source_namespaces FROM source_namespaces;

	WITH target_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @target_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			target_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #target_namespaces FROM target_namespaces;

	SELECT se.[key], se.[code] FROM [metadata].[entities] AS se
	INNER JOIN [integrator].[subscriptions] AS subs ON subs.[publisher] = se.[key]
	INNER JOIN [metadata].[entities] AS te ON subs.[subscriber] = te.[key]
	INNER JOIN #source_namespaces AS s ON se.[namespace] = s.[key]
	INNER JOIN #target_namespaces AS t ON te.[namespace] = t.[key]
	WHERE te.[code] = @type_code;

	DROP TABLE #source_namespaces;
	DROP TABLE #target_namespaces;
END
GO

CREATE PROCEDURE [integrator].[get_corresponding_target_entity]
	@source_infobase uniqueidentifier, 
	@target_infobase uniqueidentifier,
	@type_code int
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#source_namespaces') IS NOT NULL DROP TABLE #source_namespaces;
	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
	
    WITH source_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @source_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			source_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #source_namespaces FROM source_namespaces;

	WITH target_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @target_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			target_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #target_namespaces FROM target_namespaces;

	SELECT te.[key], te.[code] FROM [metadata].[entities] AS te
	INNER JOIN [integrator].[subscriptions] AS subs ON subs.[subscriber] = te.[key]
	INNER JOIN [metadata].[entities] AS se ON subs.[publisher] = se.[key]
	INNER JOIN #source_namespaces AS s ON se.[namespace] = s.[key]
	INNER JOIN #target_namespaces AS t ON te.[namespace] = t.[key]
	WHERE se.[code] = @type_code;

	DROP TABLE #source_namespaces;
	DROP TABLE #target_namespaces;
END
GO

CREATE PROCEDURE [integrator].[get_property_type_codes_lookup]
	@property uniqueidentifier,
	@target_infobase uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
	WITH target_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @target_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			target_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #target_namespaces FROM target_namespaces;

	SELECT se.[code], te.[code] FROM [integrator].[subscriptions] AS s
	INNER JOIN [metadata].[entities] AS se ON s.[publisher] = se.[key]
	INNER JOIN [metadata].[entities] AS te ON s.[subscriber] = te.[key]
	INNER JOIN #target_namespaces AS n ON te.[namespace] = n.[key]
	INNER JOIN [metadata].[relations] AS r ON r.[entity] = s.[publisher]
	WHERE r.[property] = @property;

	DROP TABLE #target_namespaces;
END
GO