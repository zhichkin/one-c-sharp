USE [Z]
GO

IF OBJECT_ID('[integrator].[get_type_codes_lookup]') IS NOT NULL
BEGIN
	DROP PROCEDURE [integrator].[get_type_codes_lookup];
END
GO

CREATE PROCEDURE [integrator].[get_type_codes_lookup]
	@source_infobase uniqueidentifier,
	@target_infobase uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	WITH
		publisher_namespaces ([owner], [key]) AS
		(
			SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @source_infobase
			UNION ALL
			SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
			INNER JOIN
				publisher_namespaces AS anchor
			ON anchor.[key] = n.[owner]
		),
		subscriber_namespaces ([owner], [key]) AS
		(
			SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @target_infobase
			UNION ALL
			SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
			INNER JOIN
				subscriber_namespaces AS anchor
			ON anchor.[key] = n.[owner]
		)
	SELECT DISTINCT
		pub.[code],
		sub.[code]
	FROM
		[integrator].[subscriptions] AS pubsub
		INNER JOIN [metadata].[entities] AS pub ON pubsub.[publisher]  = pub.[key]
		INNER JOIN [metadata].[entities] AS sub ON pubsub.[subscriber] = sub.[key]
		INNER JOIN publisher_namespaces  AS pub_ns ON pub.[namespace] = pub_ns.[key]
		INNER JOIN subscriber_namespaces AS sub_ns ON sub.[namespace] = sub_ns.[key]
	WHERE
		pub.[code] > 0 AND sub.[code] > 0 AND pub.[code] <> sub.[code];
END
GO