USE [Z]
GO

CREATE PROCEDURE [integrator].[get_property_type_codes_lookup]
	@property uniqueidentifier,
	@target_infobase uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
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

	SELECT se.[code], te.[code] FROM [integrator].[subscriptions] AS s
	INNER JOIN [metadata].[entities] AS se ON s.[publisher] = se.[key]
	INNER JOIN [metadata].[entities] AS te ON s.[subscriber] = te.[key]
	INNER JOIN #target_namespaces AS n ON te.[namespace] = n.[key]
	INNER JOIN [metadata].[relations] AS r ON r.[entity] = s.[publisher]
	WHERE r.[property] = @property;

	DROP TABLE #target_namespaces;
END
GO