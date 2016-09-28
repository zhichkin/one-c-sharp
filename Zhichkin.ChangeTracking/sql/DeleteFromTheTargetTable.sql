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
DELETE [test].[target_table]
WHERE
	[key] = @key
	AND
	@last_sync_version >= ISNULL((SELECT SYS_CHANGE_VERSION FROM CHANGETABLE(VERSION [test].[target_table], ([key]), (@key))), 0);

IF (@@ROWCOUNT = 0)
BEGIN

	DECLARE @target_version bigint;
	DECLARE @target_operation nchar(1);

    -- Obtain the complete change information for the row.
    SELECT
        @target_version   = SYS_CHANGE_VERSION,
		@target_operation = SYS_CHANGE_OPERATION
    FROM
        CHANGETABLE(CHANGES [test].[target_table], @last_sync_version)
    WHERE
        [key] = @key;

    -- Check SYS_CHANGE_VERSION to verify that it really was a conflict.
    -- Check SYS_CHANGE_OPERATION to determine the type of conflict: update-update or update-delete.
    -- The row that is specified by @key might no longer exist if it has been deleted.

	IF (@last_sync_version < @target_version AND @target_operation = N'U')
	BEGIN
		RAISERROR(N'Delete-update conflict is detected.', 16, -1);
	END

	IF (@target_operation = N'D')
	BEGIN
		RAISERROR(N'Delete-delete conflict is detected.', 16, -1);
	END
END
