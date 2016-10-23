--USE [master];
--GO

--ALTER DATABASE [Z] SET ENABLE_BROKER;
--GO

USE [Z];
GO

SELECT [is_broker_enabled], [service_broker_guid] FROM sys.databases WHERE [name] = N'Z';

CREATE MESSAGE TYPE
	[//Integrator/Message]
	VALIDATION = WELL_FORMED_XML;
GO

CREATE CONTRACT [//Integrator/Contract]
	([//Integrator/Message] SENT BY INITIATOR);
GO

CREATE QUEUE [IntegratorQueue];
CREATE SERVICE [//Integrator/Service]
	ON QUEUE [IntegratorQueue]([//Integrator/Contract]);
GO