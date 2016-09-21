USE [Z];
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