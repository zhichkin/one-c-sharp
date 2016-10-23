DECLARE @handle UNIQUEIDENTIFIER;
DECLARE @message_body NVARCHAR(4000);
DECLARE @message_type sysname;

WHILE (1=1)
  BEGIN

    BEGIN TRANSACTION;

    WAITFOR
    (RECEIVE TOP(1)
        @handle = conversation_handle,
        @message_body = message_body,
        @message_type = message_type_name
      FROM [IntegratorQueue]
    ), TIMEOUT 1000;

    IF (@@ROWCOUNT = 0)
    BEGIN
      ROLLBACK TRANSACTION;
      BREAK;
    END

    IF @message_type = N'//Integrator/Message'
    BEGIN
		SELECT @message_body;
		END CONVERSATION @handle;
    END
    ELSE IF @message_type = N'http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog'
    BEGIN
       END CONVERSATION @handle;
	   SELECT N'Конец диалога';
    END
    ELSE IF @message_type = N'http://schemas.microsoft.com/SQL/ServiceBroker/Error'
    BEGIN
       END CONVERSATION @handle;
	   SELECT N'Ошибка!';
    END
      
    COMMIT TRANSACTION;

  END
