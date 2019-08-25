USE [Z];
GO

ALTER TABLE [metadata].[entities] ADD
[is_abstract] [bit] NOT NULL DEFAULT 0x00,
[is_sealed]   [bit] NOT NULL DEFAULT 0x00;
GO

ALTER TABLE [metadata].[properties] ADD
[is_abstract]    [bit] NOT NULL DEFAULT 0x00,
[is_read_only]   [bit] NOT NULL DEFAULT 0x00,
[is_primary_key] [bit] NOT NULL DEFAULT 0x00;
GO