USE [one-c-sharp];

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = N'requests' AND type = N'U')
BEGIN
	CREATE TABLE [metadata].[requests](
		[key] [uniqueidentifier] NOT NULL,
		[version] [rowversion] NOT NULL,
		[name] [nvarchar](100) NOT NULL,
		[namespace] [uniqueidentifier] NOT NULL,
		[owner] [uniqueidentifier] NOT NULL,
		[parse_tree] [nvarchar](max) NOT NULL,
		[request_type] [uniqueidentifier] NOT NULL,
		[response_type] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_requests] PRIMARY KEY CLUSTERED 
		(
			[key] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END