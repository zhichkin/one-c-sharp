USE [Z]
GO

IF OBJECT_ID('[integrator].[get_corresponding_source_entity]') IS NOT NULL
BEGIN
	DROP PROCEDURE [integrator].[get_corresponding_source_entity];
END
GO

CREATE PROCEDURE [integrator].[get_corresponding_source_entity]
	@source_infobase uniqueidentifier, 
	@target_infobase uniqueidentifier,
	@type_code int
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#source_namespaces') IS NOT NULL DROP TABLE #source_namespaces;
	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
	
    WITH source_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @source_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			source_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #source_namespaces FROM source_namespaces;

	WITH target_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @target_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
		INNER JOIN
			target_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #target_namespaces FROM target_namespaces;

	SELECT se.[key], se.[code] FROM [metadata].[entities] AS se
	INNER JOIN [integrator].[subscriptions] AS subs ON subs.[publisher] = se.[key]
	INNER JOIN [metadata].[entities] AS te ON subs.[subscriber] = te.[key]
	INNER JOIN #source_namespaces AS s ON se.[namespace] = s.[key]
	INNER JOIN #target_namespaces AS t ON te.[namespace] = t.[key]
	WHERE te.[code] = @type_code AND se.[code] > 0;

	DROP TABLE #source_namespaces;
	DROP TABLE #target_namespaces;
END
GO