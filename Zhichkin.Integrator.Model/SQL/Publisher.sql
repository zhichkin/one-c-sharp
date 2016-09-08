USE [zhichkin];
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