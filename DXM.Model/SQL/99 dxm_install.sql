USE [Z];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dxm')
BEGIN
    -- The schema must be run in its own batch!
    EXECUTE('CREATE SCHEMA [dxm];');
END
GO

CREATE TABLE [dxm].[publications](
	[key]       [uniqueidentifier] NOT NULL,
	[version]   [rowversion]       NOT NULL,
	[name]      [nvarchar](128)    NOT NULL,
	[publisher] [uniqueidentifier] NOT NULL, -- foreign key to InfoBase
 CONSTRAINT [pk_publications] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
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

CREATE TABLE [dxm].[subscribers](
	[publication] [uniqueidentifier] NOT NULL, -- foreign key to Publication
	[subscriber]  [uniqueidentifier] NOT NULL, -- foreign key to InfoBase (subscriber)
 CONSTRAINT [pk_subscribers] PRIMARY KEY CLUSTERED 
(
	[publication] ASC, [subscriber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dxm].[subscriptions](
	[key]     [uniqueidentifier] NOT NULL,
	[version] [rowversion]       NOT NULL,
	[name]    [nvarchar](256)    NOT NULL,
	[source]  [uniqueidentifier] NOT NULL, -- foreign key to Article.Entity
	[target]  [uniqueidentifier] NOT NULL, -- foreign key to Entity
 CONSTRAINT [pk_subscription_items] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [unx_subscriptions] ON [dxm].[subscriptions]
(
	[source] ASC, [target] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
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

CREATE TABLE [dxm].[filters](
	[key]          [uniqueidentifier] NOT NULL,
	[version]      [rowversion]       NOT NULL,
	[name]         [nvarchar](256)    NOT NULL,
	[subscription] [uniqueidentifier] NOT NULL, -- foreign key to Subscription
	[property]     [uniqueidentifier] NOT NULL, -- foreign key to Subscription.Source.Property
	[value]        [nvarchar](128)    NOT NULL, -- value of Subscription.Source.Property type
 CONSTRAINT [pk_filters] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
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