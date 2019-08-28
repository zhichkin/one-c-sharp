USE [master];

IF NOT EXISTS(SELECT 1 FROM sys.databases WHERE name = N'one-c-sharp')
BEGIN
	CREATE DATABASE [one-c-sharp];
END