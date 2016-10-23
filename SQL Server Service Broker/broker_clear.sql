USE [Z];
GO

IF EXISTS(SELECT 1 FROM sys.services WHERE [name] = N'//Integrator/Service')
BEGIN
	DROP SERVICE [//Integrator/Service];
END

IF EXISTS(SELECT 1 FROM sys.service_queues WHERE [name] = N'IntegratorQueue')
BEGIN
	DROP QUEUE [IntegratorQueue];
END

IF EXISTS(SELECT 1 FROM sys.service_contracts WHERE [name] = N'//Integrator/Contract')
BEGIN
	DROP CONTRACT [//Integrator/Contract];
END

IF EXISTS(SELECT 1 FROM sys.service_message_types WHERE [name] = N'//Integrator/Message')
BEGIN
	DROP MESSAGE TYPE [//Integrator/Message];
END