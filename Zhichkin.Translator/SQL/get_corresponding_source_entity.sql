USE [zhichkin]
GO

/****** Object:  StoredProcedure [dbo].[get_corresponding_target_entity]    Script Date: 20.10.2015 13:35:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
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
		SELECT [owner], [key] FROM [dbo].[namespaces] WHERE [owner] = @source_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [dbo].[namespaces] AS n
		INNER JOIN
			source_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #source_namespaces FROM source_namespaces;

	WITH target_namespaces ([owner], [key]) AS
	(
		SELECT [owner], [key] FROM [dbo].[namespaces] WHERE [owner] = @target_infobase
		UNION ALL
		SELECT n.[owner], n.[key] FROM [dbo].[namespaces] AS n
		INNER JOIN
			target_namespaces AS anchor
		ON anchor.[key] = n.[owner]
	)
	SELECT * INTO #target_namespaces FROM target_namespaces;

	SELECT se.[key], se.[code] FROM [dbo].[entities] AS se
	INNER JOIN [integrator].[subscriptions] AS subs ON subs.[publisher] = se.[key]
	INNER JOIN [dbo].[entities] AS te ON subs.[subscriber] = te.[key]
	INNER JOIN #source_namespaces AS s ON se.[namespace] = s.[key]
	INNER JOIN #target_namespaces AS t ON te.[namespace] = t.[key]
	WHERE te.[code] = @type_code;

	DROP TABLE #source_namespaces;
	DROP TABLE #target_namespaces;
END

GO


