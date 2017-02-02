USE [Z];
GO

CREATE TABLE [integrator].[aggregates](
	[aggregate] [uniqueidentifier] NOT NULL,
	[component] [uniqueidentifier] NOT NULL,
	[connector] [uniqueidentifier] NOT NULL,
 CONSTRAINT [pk_integrator_aggregates] PRIMARY KEY CLUSTERED 
(
	[aggregate] ASC, [component] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO