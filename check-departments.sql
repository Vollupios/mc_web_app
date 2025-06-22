-- Verificar se existem departamentos no banco
SELECT 
    d.Id,
    d.Name,
    COUNT(DISTINCT u.Id) as TotalUsuarios,
    COUNT(DISTINCT doc.Id) as TotalDocumentos
FROM Departments d
LEFT JOIN AspNetUsers u ON u.DepartmentId = d.Id
LEFT JOIN Documents doc ON doc.DepartmentId = d.Id
GROUP BY d.Id, d.Name
ORDER BY d.Name;

-- Se não existirem departamentos, inserir os padrões
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Pessoal')
    INSERT INTO Departments (Name) VALUES ('Pessoal');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Fiscal')
    INSERT INTO Departments (Name) VALUES ('Fiscal');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Contábil')
    INSERT INTO Departments (Name) VALUES ('Contábil');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Cadastro')
    INSERT INTO Departments (Name) VALUES ('Cadastro');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Apoio')
    INSERT INTO Departments (Name) VALUES ('Apoio');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'TI')
    INSERT INTO Departments (Name) VALUES ('TI');
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Geral')
    INSERT INTO Departments (Name) VALUES ('Geral');
