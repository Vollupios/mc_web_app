# 🔐 Configuração de Segurança - Secrets Management

Este documento explica como configurar de forma segura as informações sensíveis da aplicação Intranet Documentos.

## 🔒 Informações Sensíveis Protegidas

As seguintes informações foram removidas dos arquivos de configuração por segurança:

- **JWT Secret**: Chave para assinatura de tokens JWT
- **Credenciais do Admin**: Email e senha do usuário administrador
- **Connection Strings**: Senhas de banco de dados (produção)
- **Email Settings**: Credenciais SMTP

## 🛠️ Configuração por Ambiente

### 📋 Desenvolvimento (User Secrets)

**Já configurado automaticamente!** Os secrets estão armazenados de forma segura via User Secrets:

```bash
# Verificar secrets configurados
dotnet user-secrets list

# Adicionar novos secrets se necessário
dotnet user-secrets set "JwtSettings:Secret" "SuaChaveSecreta"
dotnet user-secrets set "AdminUser:Email" "admin@empresa.com"
dotnet user-secrets set "AdminUser:Password" "SenhaSegura123!"
```

### 🚀 Produção (Variáveis de Ambiente)

Configure as seguintes variáveis de ambiente no servidor:

```bash
# JWT Settings
export JWT_SECRET="SuaChaveJWTSuperSecretaComMaisDe32Caracteres123456"

# Admin User
export ADMIN_EMAIL="admin@empresa.com"
export ADMIN_PASSWORD="SenhaAdminSuperSegura123!@#"

# Database (se necessário)
export CONNECTION_STRING="Server=...;Password=SenhaBanco"

# Email Settings (se configurado)
export SMTP_PASSWORD="SenhaEmail123"
```

#### Windows Server (IIS/Kestrel)
```powershell
# Definir variáveis de ambiente no Windows
[Environment]::SetEnvironmentVariable("JWT_SECRET", "SuaChaveSecreta", "Machine")
[Environment]::SetEnvironmentVariable("ADMIN_EMAIL", "admin@empresa.com", "Machine")
[Environment]::SetEnvironmentVariable("ADMIN_PASSWORD", "SenhaSegura123!", "Machine")
```

#### Linux/Docker
```yaml
# docker-compose.yml
version: '3.8'
services:
  app:
    environment:
      - JWT_SECRET=SuaChaveSecreta
      - ADMIN_EMAIL=admin@empresa.com
      - ADMIN_PASSWORD=SenhaSegura123!
```

## 🔐 Requisitos de Segurança

### JWT Secret
- **Mínimo**: 32 caracteres
- **Recomendado**: 64+ caracteres aleatórios
- **Exemplo**: `IntranetDocs2025!@#SuperSecretJwtKeyForProductionUse123456789`

### Senha do Admin
- **Mínimo**: 12 caracteres
- **Deve conter**: Maiúsculas, minúsculas, números e símbolos
- **Exemplo**: `AdminIntranet2025!@#SecurePassword`

## ⚠️ Boas Práticas

### ✅ O que FAZER:
- Usar User Secrets em desenvolvimento
- Usar variáveis de ambiente em produção
- Gerar chaves aleatórias fortes
- Rotacionar secrets periodicamente
- Documentar quais secrets são necessários

### ❌ O que NÃO fazer:
- ❌ Nunca commitar secrets no código
- ❌ Não usar senhas fracas ou padrão
- ❌ Não compartilhar secrets por email/chat
- ❌ Não usar a mesma chave em dev/prod

## 🚨 Verificação de Segurança

A aplicação agora verifica automaticamente:

1. **JWT Secret existe e tem 32+ caracteres**
2. **Credenciais do admin são fornecidas**
3. **Senha do admin tem 12+ caracteres**
4. **Falha na inicialização se secrets estão ausentes**

## 🔧 Troubleshooting

### Erro: "JWT Secret não configurado"
```bash
# Desenvolvimento
dotnet user-secrets set "JwtSettings:Secret" "SuaChaveSecreta"

# Produção
export JWT_SECRET="SuaChaveSecreta"
```

### Erro: "Email do admin não configurado"
```bash
# Desenvolvimento
dotnet user-secrets set "AdminUser:Email" "admin@empresa.com"
dotnet user-secrets set "AdminUser:Password" "SenhaSegura123!"

# Produção
export ADMIN_EMAIL="admin@empresa.com"
export ADMIN_PASSWORD="SenhaSegura123!"
```

## 📞 Suporte

Para dúvidas sobre configuração de segurança, consulte:
- Documentação oficial: `/DOCUMENTACAO-PRINCIPAL.md`
- Logs da aplicação para erros específicos
- Administrador do sistema

---
**🔒 Lembre-se: A segurança é responsabilidade de todos!**
