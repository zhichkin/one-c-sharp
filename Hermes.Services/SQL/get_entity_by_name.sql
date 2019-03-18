USE [Z]
GO

IF OBJECT_ID('[dbo].[get_entity_by_name]') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[get_entity_by_name];
END
GO

CREATE PROCEDURE [dbo].[get_entity_by_name]
	@infobase uniqueidentifier,
	@namespace_name nvarchar(128),
	@entity_name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	WITH
	namespaces ([owner], [key], [name]) AS
	(
		SELECT [owner], [key], [name] FROM [metadata].[namespaces] WHERE [owner] = @infobase
		UNION ALL
		SELECT n.[owner], n.[key], n.[name] FROM [metadata].[namespaces] AS n
		INNER JOIN namespaces AS anchor ON anchor.[key] = n.[owner]
	)
	SELECT e.[key], e.[code], e.[name] FROM [metadata].[entities] AS e
	INNER JOIN namespaces AS n ON e.[namespace] = n.[key]
	AND n.[name] = @namespace_name
	AND e.[name] = @entity_name;
END
GO