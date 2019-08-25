USE [Z];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'hermes')
BEGIN
    -- The schema must be run in its own batch!
    EXECUTE('CREATE SCHEMA [hermes];');
END
GO