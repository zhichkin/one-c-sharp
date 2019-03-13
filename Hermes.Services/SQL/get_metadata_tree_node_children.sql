USE [Z]
GO

/****** Object:  StoredProcedure [dbo].[get_metadata_tree_node_children]    Script Date: 13.03.2019 16:39:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[get_metadata_tree_node_children]
	@entity uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @empty uniqueidentifier = '00000000-0000-0000-0000-000000000000';

	SELECT
		(CASE WHEN n2.[key] = @empty THEN n1.[name] ELSE n2.[name] END) AS [Namespace],
		(CASE WHEN o.[key] = @empty THEN e.[name] ELSE o.[name] END) AS [Entity],
		(CASE WHEN o.[key] = @empty THEN '' ELSE e.[name] END) AS [NestedEntity],
		p.[name] AS [Property],
		(CASE WHEN n2.[key] = @empty THEN n1.[key] ELSE n2.[key] END) AS [NamespaceKey],
		(CASE WHEN o.[key] = @empty THEN e.[key] ELSE o.[key] END) AS [EntityKey],
		(CASE WHEN o.[key] = @empty THEN o.[key] ELSE e.[key] END) AS [NestedEntityKey],
		p.[key] AS [PropertyKey]
	FROM
		(SELECT [property] FROM [metadata].[relations] WHERE [entity] = @entity /*AND p.[name] <> '—сылка'*/) AS r
		INNER JOIN [metadata].[properties] AS p ON r.[property] = p.[key] AND NOT (p.[entity] = @entity AND p.[name] = '—сылка')
		INNER JOIN [metadata].[entities] AS e ON p.[entity] = e.[key]
		INNER JOIN [metadata].[namespaces] AS n1 ON e.[namespace] = n1.[key]
		INNER JOIN [metadata].[entities] AS o ON e.[owner] = o.[key]
		INNER JOIN [metadata].[namespaces] AS n2 ON o.[namespace] = n2.[key]
	ORDER BY
		(CASE WHEN n2.[key] = @empty THEN n1.[name] ELSE n2.[name] END), -- [Namespace]
		(CASE WHEN o.[key] = @empty THEN e.[name] ELSE o.[name] END), -- [Entity]
		(CASE WHEN o.[key] = @empty THEN '' ELSE e.[name] END), -- [NestedEntity]
		p.[name]; -- [Property]
END

GO


