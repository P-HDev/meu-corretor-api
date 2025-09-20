# MeuCorretorApi

API em .NET 9 para gestão de imóveis com upload de imagens, autenticação JWT e arquitetura em camadas (Domínio, InfraEstrutura, Service e API).

## Sumário
- [Tecnologias](#tecnologias)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Configuração Inicial](#configuração-inicial)
- [Execução](#execução)
- [Migrations / Banco de Dados](#migrations--banco-de-dados)
- [Autenticação](#autenticação)
- [Upload de Imagens](#upload-de-imagens)
- [Variáveis e Configurações](#variáveis-e-configurações)
- [Rotas / Endpoints](#rotas--endpoints)
- [Próximos Passos](#próximos-passos)

## Tecnologias
- .NET 9 (ASP.NET Core Minimal Hosting)
- Entity Framework Core + PostgreSQL (Npgsql)
- Swagger (Swashbuckle)
- JWT (Microsoft.AspNetCore.Authentication.JwtBearer)
- PBKDF2 para hashing de senha

## Estrutura do Projeto
```
Dominio/            -> Entidades e interfaces de domínio
InfraEstrutura/     -> EF Core (Contexto, Migrations, Repositórios, Storage local de imagens)
Service/            -> Serviços de aplicação (Imóveis, Autenticação) + DTOs + Mapeamentos
MeuCorretorApi/     -> Projeto Web (Controllers, Program, Configurações, Swagger)
docs/               -> Documentação (Endpoints.md)
```

## Configuração Inicial
1. Clonar o repositório.
2. Verificar `appsettings.json` (conexão PostgreSQL e JWT):
```
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=MeuCorretor;Username=postgres;Password=postgres"
}
```
3. Ajustar a string de conexão conforme seu ambiente.
4. (Opcional) Ajustar CORS para o frontend em `Cors:Origins`.
5. (Recomendado) Substituir `Jwt:Key` por valor forte (>= 32 chars) em produção.

## Execução
```bash
# Restaurar dependências
 dotnet restore

# Compilar
 dotnet build

# Rodar a API (a partir da pasta raiz da solução)
 dotnet run --project MeuCorretorApi

# Acessar Swagger
 http://localhost:5045/swagger
```
> Porta conforme configurado no `launchSettings.json` ou PublicBaseUrl.

## Migrations / Banco de Dados
Gerar nova migration (exemplo: alteração de modelo):
```bash
dotnet ef migrations add nome_migration --project InfraEstrutura --startup-project MeuCorretorApi --output-dir Migrations
```
Aplicar migrations:
```bash
dotnet ef database update --project InfraEstrutura --startup-project MeuCorretorApi
```

## Autenticação
- Registro: `POST /api/auth/register`
- Login: `POST /api/auth/login`
- Recebe um JWT que deve ser enviado no header:
```
Authorization: Bearer <token>
```
- Endpoints de escrita de imóveis exigem autenticação.

## Upload de Imagens
- Endpoint: `POST /api/imoveis/upload`
- `multipart/form-data` com campo `Imagens` (múltiplos arquivos) + demais campos.
- Imagens salvas em `wwwroot/imagens` (ou servidas via controller `/api/imagens/{fileName}` se `ServeImagesViaController=true`).
- Tipos aceitos: .jpg .jpeg .png .gif .webp

## Variáveis e Configurações
`appsettings.json` principais:
```
"PublicBaseUrl": "http://localhost:5045",         # Base para montar URLs absolutas
"ServeImagesViaController": true,                 # Se true, usa /api/imagens/... em vez de /imagens/...
"Jwt": {
  "Key": "DEV_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
  "Issuer": "MeuCorretorApi",
  "Audience": "MeuCorretorApi",
  "ExpireMinutes": 60
},
"Cors": { "Origins": ["http://localhost:4200"] }
```

## Rotas / Endpoints
Ver arquivo: `docs/Endpoints.md` ou Swagger em `/swagger`.

## Próximos Passos
- Paginação e filtros em listagem de imóveis
- Refresh token / revogação
- Remoção física de imagens ao deletar imóvel
- Validação MIME real das imagens
- Logs estruturados + observabilidade
- Testes automatizados (unit/integration)

## Licença
Defina aqui a licença (ex.: MIT) se aplicável.

