USE [Z];
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