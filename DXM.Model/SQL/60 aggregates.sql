USE [Z];
GO

CREATE TABLE [dxm].[aggregates](
	[aggregate] [uniqueidentifier] NOT NULL, -- aggregate Entity
	[component] [uniqueidentifier] NOT NULL, -- Entity included by the aggregate Entity
	[connector] [uniqueidentifier] NOT NULL, -- dependent Entity's property of the aggregate Entity type (foreign key)
 CONSTRAINT [pk_integrator_aggregates] PRIMARY KEY CLUSTERED 
(
	[aggregate] ASC, [component] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO