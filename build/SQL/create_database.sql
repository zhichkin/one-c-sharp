USE [master];
GO

CREATE DATABASE [ExchangeSetup]
ON  PRIMARY 
(NAME = N'ExchangeSetup', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\ExchangeSetup.mdf', SIZE = 5120KB, MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB)
LOG ON 
(NAME = N'ExchangeSetup_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\ExchangeSetup_log.ldf', SIZE = 1024KB, MAXSIZE = 1024GB, FILEGROWTH = 10%)
GO