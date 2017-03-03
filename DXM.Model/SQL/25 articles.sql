USE [Z];
GO

CREATE TABLE [dxm].[articles](
	[key]         [uniqueidentifier] NOT NULL,
	[version]     [rowversion]       NOT NULL,
	[name]        [nvarchar](128)    NOT NULL,
	[publication] [uniqueidentifier] NOT NULL, -- foreign key to Publication
	[entity]      [uniqueidentifier] NOT NULL, -- foreign key to Entity
	--[change_tracking_system] [int]   NOT NULL, -- change tracking system enumeration {change tracking, triggers, exchange plan, ... }
 CONSTRAINT [pk_articles] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [unx_articles] ON [dxm].[articles]
(
	[publication] ASC, [entity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO