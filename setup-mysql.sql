-- Script de configuração do MySQL para Intranet Documentos
-- Execute este script como usuário root do MySQL

-- 1. Criar banco de dados
CREATE DATABASE IF NOT EXISTS IntranetDocumentos 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- 2. Criar usuário da aplicação
-- ALTERE A SENHA ABAIXO!
CREATE USER IF NOT EXISTS 'app_user'@'localhost' IDENTIFIED BY 'CHANGE_THIS_PASSWORD_123!';

-- 3. Conceder permissões
GRANT ALL PRIVILEGES ON IntranetDocumentos.* TO 'app_user'@'localhost';

-- 4. Aplicar alterações
FLUSH PRIVILEGES;

-- 5. Verificar criação
SELECT User, Host FROM mysql.user WHERE User = 'app_user';
SHOW DATABASES LIKE 'IntranetDocumentos';

-- 6. Configurações recomendadas para produção
SET GLOBAL max_allowed_packet = 52428800; -- 50MB
SET GLOBAL innodb_buffer_pool_size = 134217728; -- 128MB (ajuste conforme memória disponível)

-- 7. Criar backup user (opcional, para backups automáticos)
CREATE USER IF NOT EXISTS 'backup_user'@'localhost' IDENTIFIED BY 'CHANGE_BACKUP_PASSWORD_456!';
GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER ON IntranetDocumentos.* TO 'backup_user'@'localhost';

FLUSH PRIVILEGES;

-- Verificação final
SELECT 
    'Database created successfully' as Status,
    SCHEMA_NAME as DatabaseName,
    DEFAULT_CHARACTER_SET_NAME as CharacterSet,
    DEFAULT_COLLATION_NAME as Collation
FROM information_schema.SCHEMATA 
WHERE SCHEMA_NAME = 'IntranetDocumentos';

SELECT 
    'Users created successfully' as Status,
    User as Username,
    Host
FROM mysql.user 
WHERE User IN ('app_user', 'backup_user');
