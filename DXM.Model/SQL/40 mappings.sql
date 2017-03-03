USE [Z];
GO

CREATE TABLE [dxm].[mappings](
	[key]     [uniqueidentifier] NOT NULL,
	[version] [rowversion]       NOT NULL,
	[name]    [nvarchar](256)    NOT NULL,
	[source]  [uniqueidentifier] NOT NULL, -- foreign key to Property
	[target]  [uniqueidentifier] NOT NULL, -- foreign key to Property
	[is_sync_key] [bit]          NOT NULL, -- binding properties
 CONSTRAINT [pk_mappings] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [unx_mappings] ON [dxm].[mappings]
(
	[source] ASC, [target] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO