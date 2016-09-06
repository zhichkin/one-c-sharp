USE [zhichkin];
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA [integrator];
GO

CREATE TABLE [integrator].[entities](
	[key] [uniqueidentifier] NOT NULL,
	[version] [rowversion] NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[last_sync_version] [bigint] NOT NULL,
	[msmq_target_queue] [nvarchar](256) NOT NULL,
 CONSTRAINT [pk_integrator_entities] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO