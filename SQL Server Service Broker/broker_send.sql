USE [Z];
GO

DECLARE @handle UNIQUEIDENTIFIER;
DECLARE @message XML;

BEGIN TRANSACTION;

BEGIN DIALOG @handle
	FROM SERVICE
	[//Integrator/Service]
	TO SERVICE
	N'//Integrator/Service'
	ON CONTRACT
	[//Integrator/Contract]
	WITH ENCRYPTION = OFF;

SELECT @message = (SELECT
	CAST([_IDRRef] AS uniqueidentifier) AS [_IDRRef]
	,[_Version]
	,CAST([_Marked] AS bit) AS [_Marked]
	,CAST([_PredefinedID] AS uniqueidentifier) AS [_PredefinedID]
	,CAST([_ParentIDRRef] AS uniqueidentifier) AS [_ParentIDRRef]
	,CAST([_Folder] AS bit) AS [_Folder]
	,[_Code]
	,[_Description]
	,CAST([_Fld66RRef] AS uniqueidentifier) AS [_Fld66RRef]
FROM [FIFO].[dbo].[_Reference11] FOR XML RAW(N'i'), ROOT(N'm'), TYPE, BINARY BASE64);

SEND ON CONVERSATION @handle
	MESSAGE TYPE
	[//Integrator/Message]
	(@message);

COMMIT TRANSACTION;
GO