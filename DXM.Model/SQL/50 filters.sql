USE [Z];
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