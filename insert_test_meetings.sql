-- Script para inserir dados de teste para reuniões
-- Execute este script no banco SQLite para criar algumas reuniões de exemplo

-- Inserir reuniões de teste
INSERT OR IGNORE INTO Reunioes (
    Id, Titulo, Data, HoraInicio, HoraFim, TipoReuniao, Sala, Veiculo, 
    LinkOnline, Empresa, Status, Observacoes, DataCriacao, ResponsavelUserId
) VALUES 
-- Reunião 1 - Interna
(1, 'Reunião de Planejamento', '2025-06-25', '09:00:00', '10:30:00', 0, 0, NULL, 
 NULL, NULL, 0, 'Reunião para planejamento mensal', datetime('now'), 
 (SELECT Id FROM AspNetUsers LIMIT 1)),

-- Reunião 2 - Externa  
(2, 'Reunião com Cliente ABC', '2025-06-26', '14:00:00', '16:00:00', 1, 1, 0,
 NULL, 'Cliente ABC Ltda', 0, 'Apresentação de propostas', datetime('now'),
 (SELECT Id FROM AspNetUsers LIMIT 1)),

-- Reunião 3 - Online
(3, 'Reunião Online - Treinamento', '2025-06-27', '10:00:00', '12:00:00', 2, NULL, NULL,
 'https://meet.google.com/abc-defg-hij', NULL, 0, 'Treinamento da equipe', datetime('now'),
 (SELECT Id FROM AspNetUsers LIMIT 1)),

-- Reunião 4 - Hoje
(4, 'Reunião de Status', '2025-06-21', '15:00:00', '16:00:00', 0, 2, NULL,
 NULL, NULL, 0, 'Status semanal dos projetos', datetime('now'),
 (SELECT Id FROM AspNetUsers LIMIT 1)),

-- Reunião 5 - Amanhã
(5, 'Reunião com Fornecedor XYZ', '2025-06-22', '11:00:00', '12:30:00', 1, 0, 1,
 NULL, 'Fornecedor XYZ S/A', 0, 'Negociação de contrato', datetime('now'),
 (SELECT Id FROM AspNetUsers LIMIT 1));

-- Inserir alguns participantes de exemplo
INSERT OR IGNORE INTO ReuniaoParticipantes (
    Id, ReuniaoId, Nome, RamalId, DepartamentoId
) VALUES
(1, 1, 'João Silva', NULL, 1),
(2, 1, 'Maria Santos', NULL, 2),
(3, 2, 'Pedro Costa', NULL, 1),
(4, 3, 'Ana Oliveira', NULL, 3),
(5, 4, 'Carlos Ferreira', NULL, 1),
(6, 5, 'Lucia Rodrigues', NULL, 2);
