USE [Z];
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