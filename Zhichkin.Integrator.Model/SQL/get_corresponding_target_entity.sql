USE [Z]
GO

IF OBJECT_ID('[integrator].[get_corresponding_target_entity]') IS NOT NULL
BEGIN
	DROP PROCEDURE [integrator].[get_corresponding_target_entity];
END
GO

CREATE PROCEDURE [integrator].[get_corresponding_target_entity]
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

	SELECT te.[key], te.[code] FROM [metadata].[entities] AS te
	INNER JOIN [integrator].[subscriptions] AS subs ON subs.[subscriber] = te.[key]
	INNER JOIN [metadata].[entities] AS se ON subs.[publisher] = se.[key]
	INNER JOIN #source_namespaces AS s ON se.[namespace] = s.[key]
	INNER JOIN #target_namespaces AS t ON te.[namespace] = t.[key]
	WHERE se.[code] = @type_code AND te.[code] > 0;

	DROP TABLE #source_namespaces;
	DROP TABLE #target_namespaces;
END
GO