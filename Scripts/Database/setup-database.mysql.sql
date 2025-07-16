-- ============================================
-- MySQL Setup Script for Intranet Documentos
-- File: setup-database.mysql.sql
-- Database Engine: MySQL 8.0+
-- ============================================

-- Use MySQL syntax explicitly
SET sql_mode = 'STRICT_TRANS_TABLES,NO_ZERO_DATE,NO_ZERO_IN_DATE,ERROR_FOR_DIVISION_BY_ZERO';

-- 1. Create database with proper charset
CREATE DATABASE IF NOT EXISTS `IntranetDocumentos` 
DEFAULT CHARACTER SET utf8mb4 
DEFAULT COLLATE utf8mb4_unicode_ci;

-- 2. Create application user
-- WARNING: Change password before production deployment!
CREATE USER IF NOT EXISTS `app_user`@`localhost` IDENTIFIED BY 'CHANGE_THIS_PASSWORD_123!';

-- 3. Grant privileges to application user
GRANT ALL PRIVILEGES ON `IntranetDocumentos`.* TO `app_user`@`localhost`;

-- 4. Apply changes
FLUSH PRIVILEGES;

-- 5. Verify user and database creation
SELECT `User`, `Host` FROM `mysql`.`user` WHERE `User` = 'app_user';
SHOW DATABASES LIKE 'IntranetDocumentos';

-- 6. Production-recommended configurations
-- Adjust these values based on your server capacity
SET GLOBAL max_allowed_packet = 52428800; -- 50MB for file uploads
SET GLOBAL innodb_buffer_pool_size = 134217728; -- 128MB (adjust based on available memory)
SET GLOBAL innodb_log_file_size = 67108864; -- 64MB

-- 7. Create backup user (optional, for automated backups)
-- WARNING: Change backup password before production deployment!
CREATE USER IF NOT EXISTS `backup_user`@`localhost` IDENTIFIED BY 'CHANGE_BACKUP_PASSWORD_456!';
GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER ON `IntranetDocumentos`.* TO `backup_user`@`localhost`;

-- Apply backup user privileges
FLUSH PRIVILEGES;

-- ============================================
-- Final verification queries
-- ============================================

-- Check database creation
SELECT 
    'Database created successfully' as `Status`,
    `SCHEMA_NAME` as `DatabaseName`,
    `DEFAULT_CHARACTER_SET_NAME` as `CharacterSet`,
    `DEFAULT_COLLATION_NAME` as `Collation`
FROM `information_schema`.`SCHEMATA` 
WHERE `SCHEMA_NAME` = 'IntranetDocumentos';

-- Check user creation
SELECT 
    'Users created successfully' as `Status`,
    `User` as `Username`,
    `Host`,
    `authentication_string` as `HasPassword`
FROM `mysql`.`user` 
WHERE `User` IN ('app_user', 'backup_user');

-- Show grants for application user
SHOW GRANTS FOR `app_user`@`localhost`;

-- Show grants for backup user
SHOW GRANTS FOR `backup_user`@`localhost`;

-- ============================================
-- Connection string example for .NET application:
-- Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=CHANGE_THIS_PASSWORD_123!;
-- ============================================
