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

# Catálogo de Endpoints - MeuCorretorApi

Base URL (desenvolvimento padrão): `http://localhost:5045`

## Autenticação
Endpoints públicos para registro e login.

### POST /api/auth/register
Registra um novo usuário.
Body (JSON):
```
{
  "nome": "João da Silva",
  "email": "joao@example.com",
  "senha": "SenhaSegura123",
  "telefone": "+5511999999999"
}
```
Responses:
- 200 OK: `AuthResponseDto` (contém token JWT)
- 400 BadRequest: email já utilizado ou validação

### POST /api/auth/login
Efetua login.
Body:
```
{
  "email": "joao@example.com",
  "senha": "SenhaSegura123"
}
```
Responses:
- 200 OK: `AuthResponseDto`
- 401 Unauthorized: credenciais inválidas

### AuthResponseDto
```
{
  token: string,
  expiresAtUtc: string (ISO UTC),
  user: { id: number, nome: string, email: string, telefone: string }
}
```

### Segurança
Endpoints protegidos exigem header:
```
Authorization: Bearer <TOKEN>
```
Protegidos atualmente:
- POST /api/imoveis
- POST /api/imoveis/upload
- PUT /api/imoveis/{id}
- DELETE /api/imoveis/{id}

---
## Imagens
Quando `ServeImagesViaController=true`, as URLs de imagens retornadas apontam para `/api/imagens/{fileName}`.
Caso contrário, as imagens são servidas diretamente via static files em `/imagens/{fileName}`.

### GET /api/imagens/{fileName}
Retorna a imagem.
- 200 OK (image/*)
- 404 NotFound

### HEAD /api/imagens/{fileName}
Verifica existência.
- 200 OK
- 404 NotFound

---
## Imóveis
Recurso raiz: `/api/imoveis`

### GET /api/imoveis
Retorna a lista de imóveis.
- 200 OK: `[ImovelDto]`

### GET /api/imoveis/{id}
Retorna um imóvel específico.
- 200 OK: `ImovelDto`
- 404 NotFound

### POST /api/imoveis (PROTEGIDO)
Cria imóvel usando URLs de imagens já existentes.
- Body (JSON): `CreateImovelDto`
- 201 Created + Location header
- 400 BadRequest
- 401 Unauthorized (sem token)

### POST /api/imoveis/upload (PROTEGIDO)
Cria imóvel com upload de arquivos de imagem.
- Content-Type: multipart/form-data
- Campos (FormData):
    - Titulo, Endereco, Descricao, Status, Preco, Area, Quartos, Banheiros, Suites, Vagas
    - Imagens (arquivos múltiplos)
- 201 Created + Location
- 400 BadRequest
- 401 Unauthorized

### PUT /api/imoveis/{id} (PROTEGIDO)
Atualiza dados e imagens (URLs).
- Body (JSON): `UpdateImovelDto`
- 204 NoContent
- 404 NotFound
- 401 Unauthorized

### DELETE /api/imoveis/{id} (PROTEGIDO)
Remove imóvel.
- 204 NoContent
- 404 NotFound
- 401 Unauthorized

---
## Contratos (DTOs)

### ImovelDto
```
{
  id: number,
  titulo: string,
  endereco: string,
  descricao: string,
  status: string,
  preco: number,
  area: number,
  quartos: number,
  banheiros: number,
  suites: number,
  vagas: number,
  imagens: [{ id: number, url: string }]
}
```

### CreateImovelDto
```
{
  titulo: string,
  endereco: string,
  descricao?: string,
  status: string,
  preco: number,
  area?: number,
  quartos?: number,
  banheiros?: number,
  suites?: number,
  vagas?: number,
  imagens: [{ url: string }]
}
```

### UpdateImovelDto
```
{
  titulo: string,
  endereco: string,
  descricao: string,
  status: string,
  preco: number,
  area: number,
  quartos: number,
  banheiros: number,
  suites: number,
  vagas: number,
  imagens: [{ id?: number, url: string }]
}
```

### CreateImovelUploadDto (multipart)
Campos: ver POST /api/imoveis/upload.

---
## CORS
Permitido por padrão: `http://localhost:4200`. Ajustar em `appsettings.json` > `Cors:Origins`.

## JWT Config (appsettings.json)
```
"Jwt": {
  "Key": "DEV_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
  "Issuer": "MeuCorretorApi",
  "Audience": "MeuCorretorApi",
  "ExpireMinutes": 60
}
```

## Tipos de Imagem Aceitos (upload)
.jpg .jpeg .png .gif .webp

## Comportamento de URL de Imagem
- `ServeImagesViaController=true`: `http://host:porta/api/imagens/{nome}`
- Caso contrário: `http://host:porta/imagens/{nome}`

## Próximos Passos Recomendados
- Paginação e filtros em GET /api/imoveis
- Soft delete / auditoria
- Refresh token e revogação
- Remoção física de imagens ao deletar
- Validação MIME real no upload

---
Consulte `/swagger` para documentação interativa.
