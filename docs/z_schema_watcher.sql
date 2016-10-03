USE [FIFO];
GO

CREATE TABLE [Z_schema_watcher]
(
    EventDate    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    EventType    NVARCHAR(64),
    EventDDL     NVARCHAR(MAX),
    EventXML     XML,
    DatabaseName NVARCHAR(255),
    SchemaName   NVARCHAR(255),
    ObjectName   NVARCHAR(255),
    HostName     VARCHAR(64),
    IPAddress    VARCHAR(32),
    ProgramName  NVARCHAR(255),
    LoginName    NVARCHAR(255)
);
GO

CREATE TRIGGER Z_schema_watcher_trigger
    ON DATABASE
    FOR CREATE_TABLE, RENAME, DROP_TABLE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE
        @EventData XML = EVENTDATA();
 
    DECLARE 
        @ip VARCHAR(32) =
        (
            SELECT client_net_address
                FROM sys.dm_exec_connections
                WHERE session_id = @@SPID
        );
 
    INSERT [Z_schema_watcher]
    (
        EventType,
        EventDDL,
        EventXML,
        DatabaseName,
        SchemaName,
        ObjectName,
        HostName,
        IPAddress,
        ProgramName,
        LoginName
    )
    SELECT
        @EventData.value('(/EVENT_INSTANCE/EventType)[1]',   'NVARCHAR(100)'), 
        @EventData.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'NVARCHAR(MAX)'),
        @EventData,
        DB_NAME(),
        @EventData.value('(/EVENT_INSTANCE/SchemaName)[1]',  'NVARCHAR(255)'), 
        @EventData.value('(/EVENT_INSTANCE/ObjectName)[1]',  'NVARCHAR(255)'),
        HOST_NAME(),
        @ip,
        PROGRAM_NAME(),
        SUSER_SNAME();
END
GO

SELECT
	EventDate,
	EventType,
	ObjectName,
	EventXML.value('(/EVENT_INSTANCE/NewObjectName)[1]', 'NVARCHAR(255)') AS [NewObjectName]
FROM
	[Z_schema_watcher]
WHERE
	EventXML.value('(/EVENT_INSTANCE/ObjectType)[1]', 'NVARCHAR(255)') = 'TABLE';