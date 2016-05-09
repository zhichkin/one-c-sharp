USE [zhichkin]
GO

/****** Object: Table [dbo].[fields] Script Date: 28.07.2015 12:22:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[fields] (
    [key]            UNIQUEIDENTIFIER NOT NULL,
    [version]        ROWVERSION       NOT NULL,
    [name]           NVARCHAR (100)   NOT NULL,
    [class]          UNIQUEIDENTIFIER NOT NULL,
    [purpose]        INT              NOT NULL,
    [type]           NVARCHAR (16)    NOT NULL,
    [length]         INT              NOT NULL,
    [precision]      INT              NOT NULL,
    [scale]          INT              NOT NULL,
    [is_nullable]    BIT              NOT NULL,
	[is_primary_key] BIT              NOT NULL,
	[key_ordinal]    TINYINT          NOT NULL
	CONSTRAINT [PK_fields] PRIMARY KEY CLUSTERED 
	(
		[key] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO