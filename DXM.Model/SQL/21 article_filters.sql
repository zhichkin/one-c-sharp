USE [Z];
GO

CREATE TABLE [dxm].[article_filters](
	[article]     uniqueidentifier NOT NULL,
	[property]    uniqueidentifier NOT NULL,
	[operator]    int              NOT NULL,
	[type]        uniqueidentifier NOT NULL,
	[value]       varbinary(256)   NOT NULL,
	CONSTRAINT [pk_article_filters] PRIMARY KEY CLUSTERED 
	(
		[article] ASC, [property] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
