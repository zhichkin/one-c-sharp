USE [one-c-sharp];

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = N'fields' AND type = N'U')
BEGIN
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
END

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = N'ncx_fields_property' AND object_id = OBJECT_ID(N'metadata.fields'))
BEGIN
	CREATE NONCLUSTERED INDEX ncx_fields_property ON [metadata].[fields] ([property]);
END