DECLARE @key int;
DECLARE @value nvarchar(100);
DECLARE @source varbinary(128);
DECLARE @last_sync_version bigint;

--SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

IF (@last_sync_version < CHANGE_TRACKING_MIN_VALID_VERSION(OBJECT_ID(N'[test].[target_table]')))
BEGIN
	RAISERROR(N'Last synchronization version is too old.', 16, -1);
END

WITH CHANGE_TRACKING_CONTEXT (@source)
INSERT [test].[target_table]
(
	[key], [value]
)
VALUES
(
	@key, @value
);
SELECT CHANGE_TRACKING_CURRENT_VERSION();