-- Script para inserir dados de teste para Analytics
-- Este script cria dados de exemplo para testar o dashboard

-- Verificar se existem departamentos e criar alguns básicos se necessário
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'TI')
BEGIN
    INSERT INTO Departments (Name, Description, CreatedAt) VALUES ('TI', 'Tecnologia da Informação', GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Pessoal')
BEGIN
    INSERT INTO Departments (Name, Description, CreatedAt) VALUES ('Pessoal', 'Recursos Humanos', GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Fiscal')
BEGIN
    INSERT INTO Departments (Name, Description, CreatedAt) VALUES ('Fiscal', 'Departamento Fiscal', GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Geral')
BEGIN
    INSERT INTO Departments (Name, Description, CreatedAt) VALUES ('Geral', 'Documentos Gerais', GETDATE())
END

-- Inserir alguns documentos de teste (apenas se não existirem)
DECLARE @TIId INT = (SELECT TOP 1 Id FROM Departments WHERE Name = 'TI')
DECLARE @PessoalId INT = (SELECT TOP 1 Id FROM Departments WHERE Name = 'Pessoal')
DECLARE @FiscalId INT = (SELECT TOP 1 Id FROM Departments WHERE Name = 'Fiscal')
DECLARE @GeralId INT = (SELECT TOP 1 Id FROM Departments WHERE Name = 'Geral')
DECLARE @AdminUserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE NormalizedUserName = 'ADMIN@COMPANY.COM' OR UserName LIKE '%admin%')

-- Se não temos usuário admin, usar o primeiro usuário disponível
IF @AdminUserId IS NULL
    SET @AdminUserId = (SELECT TOP 1 Id FROM AspNetUsers)

-- Inserir documentos de teste apenas se não existirem
IF NOT EXISTS (SELECT 1 FROM Documents WHERE Title = 'Manual do Sistema')
BEGIN
    INSERT INTO Documents (Title, Description, OriginalFileName, StoredFileName, ContentType, FileSize, DepartmentId, UploaderUserId, UploadDate, IsActive)
    VALUES 
    ('Manual do Sistema', 'Manual de uso do sistema interno', 'manual_sistema.pdf', 'test_manual_' + CAST(NEWID() AS NVARCHAR(36)) + '.pdf', 'application/pdf', 1024000, @TIId, @AdminUserId, DATEADD(DAY, -30, GETDATE()), 1),
    ('Política de RH', 'Políticas internas de recursos humanos', 'politica_rh.pdf', 'test_politica_' + CAST(NEWID() AS NVARCHAR(36)) + '.pdf', 'application/pdf', 512000, @PessoalId, @AdminUserId, DATEADD(DAY, -20, GETDATE()), 1),
    ('Relatório Fiscal', 'Relatório mensal fiscal', 'relatorio_fiscal.xlsx', 'test_relatorio_' + CAST(NEWID() AS NVARCHAR(36)) + '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 256000, @FiscalId, @AdminUserId, DATEADD(DAY, -10, GETDATE()), 1),
    ('Comunicado Geral', 'Comunicado para todos os colaboradores', 'comunicado.pdf', 'test_comunicado_' + CAST(NEWID() AS NVARCHAR(36)) + '.pdf', 'application/pdf', 128000, @GeralId, @AdminUserId, DATEADD(DAY, -5, GETDATE()), 1)
END

-- Inserir logs de download de teste
DECLARE @Doc1Id INT = (SELECT TOP 1 Id FROM Documents WHERE Title = 'Manual do Sistema')
DECLARE @Doc2Id INT = (SELECT TOP 1 Id FROM Documents WHERE Title = 'Política de RH')
DECLARE @Doc3Id INT = (SELECT TOP 1 Id FROM Documents WHERE Title = 'Relatório Fiscal')
DECLARE @Doc4Id INT = (SELECT TOP 1 Id FROM Documents WHERE Title = 'Comunicado Geral')

-- Inserir logs de download apenas se os documentos existirem e não houver logs ainda
IF @Doc1Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DocumentDownloadLogs WHERE DocumentId = @Doc1Id)
BEGIN
    INSERT INTO DocumentDownloadLogs (DocumentId, UserId, DownloadDate, UserAgent, IpAddress)
    VALUES 
    (@Doc1Id, @AdminUserId, DATEADD(DAY, -25, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0', '192.168.1.100'),
    (@Doc1Id, @AdminUserId, DATEADD(DAY, -20, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0', '192.168.1.101'),
    (@Doc1Id, @AdminUserId, DATEADD(DAY, -15, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0', '192.168.1.102')
END

IF @Doc2Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DocumentDownloadLogs WHERE DocumentId = @Doc2Id)
BEGIN
    INSERT INTO DocumentDownloadLogs (DocumentId, UserId, DownloadDate, UserAgent, IpAddress)
    VALUES 
    (@Doc2Id, @AdminUserId, DATEADD(DAY, -18, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/91.0', '192.168.1.103'),
    (@Doc2Id, @AdminUserId, DATEADD(DAY, -12, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/91.0', '192.168.1.104')
END

IF @Doc3Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DocumentDownloadLogs WHERE DocumentId = @Doc3Id)
BEGIN
    INSERT INTO DocumentDownloadLogs (DocumentId, UserId, DownloadDate, UserAgent, IpAddress)
    VALUES 
    (@Doc3Id, @AdminUserId, DATEADD(DAY, -8, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Firefox/89.0', '192.168.1.105'),
    (@Doc3Id, @AdminUserId, DATEADD(DAY, -6, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Firefox/89.0', '192.168.1.106'),
    (@Doc3Id, @AdminUserId, DATEADD(DAY, -4, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Firefox/89.0', '192.168.1.107'),
    (@Doc3Id, @AdminUserId, DATEADD(DAY, -2, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Firefox/89.0', '192.168.1.108')
END

IF @Doc4Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DocumentDownloadLogs WHERE DocumentId = @Doc4Id)
BEGIN
    INSERT INTO DocumentDownloadLogs (DocumentId, UserId, DownloadDate, UserAgent, IpAddress)
    VALUES 
    (@Doc4Id, @AdminUserId, DATEADD(DAY, -3, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0', '192.168.1.109'),
    (@Doc4Id, @AdminUserId, DATEADD(DAY, -1, GETDATE()), 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0', '192.168.1.110')
END

-- Inserir algumas reuniões de teste se não existirem
IF NOT EXISTS (SELECT 1 FROM Reunioes WHERE Titulo = 'Reunião de Planejamento TI')
BEGIN
    INSERT INTO Reunioes (Titulo, Descricao, DataHora, Local, Status, OrganizadorId, CreatedAt)
    VALUES 
    ('Reunião de Planejamento TI', 'Reunião de planejamento mensal da TI', DATEADD(DAY, -15, GETDATE()), 'Sala de Reuniões 1', 2, @AdminUserId, DATEADD(DAY, -20, GETDATE())),
    ('Reunião de RH', 'Discussão sobre políticas de RH', DATEADD(DAY, -10, GETDATE()), 'Sala de Reuniões 2', 2, @AdminUserId, DATEADD(DAY, -12, GETDATE())),
    ('Reunião Fiscal', 'Revisão do fechamento mensal', DATEADD(DAY, -5, GETDATE()), 'Sala da Contabilidade', 2, @AdminUserId, DATEADD(DAY, -7, GETDATE())),
    ('Reunião Geral', 'Comunicados gerais da empresa', DATEADD(DAY, 3, GETDATE()), 'Auditório', 1, @AdminUserId, DATEADD(DAY, -3, GETDATE()))
END

PRINT 'Dados de teste para analytics inseridos com sucesso!'
