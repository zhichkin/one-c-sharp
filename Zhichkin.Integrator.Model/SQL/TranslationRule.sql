USE [zhichkin]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'integrator')
BEGIN
    -- The schema must be run in its own batch!
    EXECUTE('CREATE SCHEMA [integrator];');
END
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

