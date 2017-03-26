USE [Z];
GO

CREATE TABLE [dxm].[publication_properties](
	[key]         uniqueidentifier NOT NULL,
	[version]     rowversion       NOT NULL,
	[publication] uniqueidentifier NOT NULL,
	[name]        nvarchar(128)    NOT NULL,
	[type]        uniqueidentifier NOT NULL,
	[value]       varbinary(256)   NOT NULL,
	[purpose]     int              NOT NULL,
	CONSTRAINT [pk_publication_properties] PRIMARY KEY CLUSTERED 
	(
		[key] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [nnx_publication_properties] ON [dxm].[publication_properties]
(
	[publication] ASC
)
GO
