USE [zhichkin]
GO

/****** Object:  StoredProcedure [dbo].[get_corresponding_target_class]    Script Date: 20.10.2015 13:35:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [integrator].[get_property_type_codes_lookup]
	@property uniqueidentifier,
	@target_infobase uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#target_namespaces') IS NOT NULL DROP TABLE #target_namespaces;
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

	SELECT se.[code], te.[code] FROM [integrator].[subscriptions] AS s
	INNER JOIN [dbo].[entities] AS se ON s.[publisher] = se.[key]
	INNER JOIN [dbo].[entities] AS te ON s.[subscriber] = te.[key]
	INNER JOIN #target_namespaces AS n ON te.[namespace] = n.[key]
	INNER JOIN [dbo].[relations] AS r ON r.[entity] = s.[publisher]
	WHERE r.[property] = @property;

	DROP TABLE #target_namespaces;
END

GO