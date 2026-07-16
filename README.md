# AuxiAPI

API REST em **.NET 10** para consulta de dados do domínio condominial, desenvolvida no desafio **StartAPI - Da Lógica à Prática na Criação de API**.

O projeto tem como objetivo praticar a construção de uma API REST aplicando conceitos de HTTP, JSON, autenticação, separação de responsabilidades, acesso a banco de dados, tratamento de erros, cache persistente, documentação e testes automatizados.

Nesta etapa, a API utiliza a estrutura de dados **Atlas** como origem principal e expõe endpoints para consulta de **Condomínios** e **Unidades**. 

---

## Status do projeto

| Área | Status |
|---|---|
| Endpoint de Condomínios | Implementado |
| Endpoint de Unidades | Implementado |
| Endpoint de Blocos/Torres | Futuro |
| Listagem paginada | Implementada |
| Busca de condomínio por `codcondom` | Implementada |
| Busca de unidade por `idEconomia` | Implementada |
| Filtros de condomínios por CNPJ e nome | Implementados |
| Filtros de unidades por `codCondom` e nome do condômino | Implementados |
| Estrutura Atlas de Condomínios | Implementada |
| Estrutura Atlas de Unidades | Implementada |
| Autenticação JWT | Implementada |
| Token automático em ambiente Development | Implementado |
| Injeção automática de Authorization em Development | Implementada |
| Endpoint `/dev/token` para apoio/debug | Implementado |
| Tratamento global de erros | Implementado |
| Cache persistente em PostgreSQL/Supabase | Implementado |
| Cache de condomínios por `codcondom`, CNPJ e nome | Implementado |
| Cache de unidades por `idEconomia`, `codCondom` e nome do condômino | Implementado |
| Invalidação automática de cache por trigger | Implementada |
| Página HTML de consulta | Implementada como apoio |
| Testes automatizados | Implementados e validados |

---

## Contexto do desafio

O desafio propõe o desenvolvimento de endpoints de consulta para mapear o fluxo estrutural de dados do domínio condominial:

1. **Condomínios**
2. **Unidades**

O endpoint de **Condomínios** foi priorizado inicialmente para consolidar a base técnica da API. Em seguida, o endpoint de **Unidades** foi implementado seguindo o mesmo padrão arquitetural, reaproveitando autenticação, cache persistente, tratamento de erros, paginação, repositórios, services, DTOs e testes automatizados.

A estrutura atual considera as tabelas Atlas:

- `atlas_condominios`;
- `atlas_blocos`;
- `atlas_unidades`.

Nesta fase, a API expõe dados de:

- `public.atlas_condominios`;
- `public.atlas_unidades`.
- 
---

## Escopo atual

O escopo atual cobre consultas de condomínios e unidades por meio dos endpoints:

```http
GET /api/condominios
GET /api/condominios/{codcondom}

GET /api/unidades
GET /api/unidades/{idEconomia}
```

A API permite:

- listar condomínios com paginação;
- consultar um condomínio por `codcondom`;
- filtrar condomínios por CNPJ com ou sem máscara;
- filtrar condomínios por nome ignorando diferenças de caixa e acentuação;
- listar unidades com paginação;
- consultar uma unidade por `idEconomia`;
- filtrar unidades por código do condomínio;
- filtrar unidades por nome do condômino ignorando diferenças de caixa e acentuação;
- retornar dados no contrato baseado na estrutura Atlas;
- retornar erros padronizados;
- proteger os endpoints com JWT;
- automatizar o token em ambiente de desenvolvimento;
- cachear consultas relevantes;
- invalidar cache automaticamente quando as tabelas Atlas relacionadas são alteradas;
- validar o comportamento da API por testes unitários, de persistência e de integração.

---

## Tecnologias utilizadas

- **.NET 10**
- **C#**
- **ASP.NET Core**
- **Entity Framework Core**
- **PostgreSQL**
- **Supabase**
- **Supabase Auth**
- **JWT Bearer Authentication**
- **Swagger / OpenAPI**
- **xUnit**
- **Moq**
- **Testcontainers**
- **Docker**
- **HTML, CSS e JavaScript** para página simples de apoio à consulta manual

---

## Arquitetura e estrutura do projeto

O AuxiAPI utiliza uma arquitetura baseada em camadas, seguindo princípios de separação de responsabilidades para garantir organização, testabilidade e manutenção.

```text
AuxiAPI/
├── database/
│   └── atlas/
│       ├── csv/          # Arquivos CSV usados para carga das tabelas Atlas
│       └── sql/          # Scripts SQL de criação, validação e cache Atlas
│
├── src/ (Projeto Web API)
│   ├── Common/           # Utilitários, mensagens e normalizações compartilhadas
│   ├── Contexts/         # DbContext, configurações do EF Core e funções PostgreSQL
│   ├── Controllers/      # Endpoints REST da API
│   ├── DTOs/             # Contratos de entrada e saída
│   ├── Entities/         # Entidades persistidas no banco
│   ├── Middlewares/      # Tratamento global de erro e interceptores HTTP
│   │   └── DevTokenInjectionMiddleware.cs
│   ├── Migrations/       # Histórico de evolução da estrutura do banco
│   ├── Repositories/     # Acesso a dados
│   ├── Security/         # JWT e token automático de desenvolvimento
│   │   ├── DevTokenOptions.cs
│   │   ├── IDevTokenService.cs
│   │   ├── SupabaseAuthResponse.cs
│   │   ├── SupabaseDevTokenService.cs
│   │   └── DevTokenStartupService.cs
│   ├── Services/         # Regras da aplicação
│   ├── wwwroot/          # Página HTML de apoio
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
├── test/ (Suíte de Testes)
│   ├── ControllersTest/
│   ├── DTOsTest/
│   ├── IntegrationTest/
│   ├── MiddlewaresTest/
│   ├── RepositoriesTest/
│   ├── ServicesTest/
│   └── TestInfrastructure/
```

---

## Responsabilidades das camadas

| Camada | Responsabilidade |
|---|---|
| Controller | Recebe requisições HTTP e retorna respostas |
| Service | Aplica validações, regras de paginação, cache e mapeamento para DTO |
| Repository | Consulta o banco com Entity Framework Core |
| DTOs | Definem contratos de entrada e saída da API |
| Mapper | Converte entidades Atlas para DTOs de resposta |
| Middleware | Padroniza tratamento de erros e, em Development, injeta token automaticamente em chamadas `/api` |
| Security | Centraliza autenticação JWT e automação de token em ambiente de desenvolvimento |
| CacheRepository | Lê e grava registros na tabela de cache |
| TestInfrastructure | Fornece autenticação fake e banco PostgreSQL em container para testes |

### Controller

A camada de controller expõe os endpoints HTTP da API.

Endpoints disponíveis nesta fase:

```http
GET /api/condominios
GET /api/condominios/{codcondom}
GET /api/unidades
GET /api/unidades/{idEconomia}
```

O controller não concentra regra de negócio. Ele recebe a requisição, chama o service e retorna a resposta adequada.

### Service

A camada de service concentra as regras da aplicação.

Ela é responsável por:

- validar parâmetros recebidos;
- aplicar regra de paginação;
- consultar cache persistente;
- salvar respostas cacheadas quando aplicável;
- mapear entidades Atlas para DTOs;
- tratar cenários de recurso não encontrado.

### Repository

A camada de repository acessa o banco de dados com Entity Framework Core.

No endpoint de Condomínios, ela executa:

- busca por `codcondom`;
- listagem paginada;
- filtro por CNPJ;
- filtro por nome;
- consultas de leitura usando `AsNoTracking()`.

No endpoint de Unidades, ela executa:

- busca por `idEconomia`;
- listagem paginada;
- filtro por `codCondom`;
- filtro por nome do condômino;
- consultas de leitura usando `AsNoTracking()`.

As origens atuais dos dados são:

```text
public.atlas_condominios
public.atlas_unidades
```

### Security

A camada `Security` concentra a lógica relacionada à autenticação e ao token automático de desenvolvimento.

Arquivos principais:

| Arquivo | Responsabilidade |
|---|---|
| `DevTokenOptions.cs` | Define as configurações do token automático |
| `IDevTokenService.cs` | Contrato para serviços capazes de obter e expor token de desenvolvimento |
| `SupabaseAuthResponse.cs` | Modelo da resposta retornada pelo Supabase Auth |
| `SupabaseDevTokenService.cs` | Autentica no Supabase Auth e armazena o token em memória |
| `DevTokenStartupService.cs` | Carrega o token automaticamente quando a API inicia em Development |

### DTOs

Os DTOs definem os contratos de entrada e saída da API.

DTOs principais desta fase:

```text
AtlasCondominioDto
AtlasUnidadeDto
VisualizarCondominioQuery
VisualizarUnidadeQuery
ResultadoPaginadoDto<T>
```

A API não retorna diretamente as entidades do banco. Isso permite controlar o contrato público da API, mesmo quando a estrutura interna da aplicação evolui.

---

## Endpoint de Condomínios

### Listar condomínios

```http
GET /api/condominios
```

Retorna uma lista paginada de condomínios.

Exemplo:

```http
GET /api/condominios?pagina=1
```

### Buscar condomínio por `codcondom`

```http
GET /api/condominios/{codcondom}
```

Exemplo:

```http
GET /api/condominios/5396
```

### Filtros disponíveis

| Filtro | Query param | Observação |
|---|---|---|
| CNPJ | `cnpj` | Aceita com ou sem máscara |
| Nome | `nomeCondom` | Ignora caixa e acentuação |
| Página | `pagina` | Página mínima: 1 |

Exemplos:

```http
GET /api/condominios?cnpj=17474690000113
GET /api/condominios?cnpj=17.474.690/0001-13
GET /api/condominios?nomeCondom=solar
GET /api/condominios?nomeCondom=solar&pagina=2
```

O tamanho da página é fixo em **10 itens**.

A busca por código do condomínio não é feita por query string. Para consultar um condomínio específico, use:

```http
GET /api/condominios/{codcondom}
```

---

## Endpoint de Unidades

### Listar unidades

```http
GET /api/unidades
```

Retorna uma lista paginada de unidades.

Exemplo:

```http
GET /api/unidades?pagina=1
```

### Buscar unidade por `idEconomia`

```http
GET /api/unidades/{idEconomia}
```

Exemplo:

```http
GET /api/unidades/123
```

### Filtros disponíveis

| Filtro | Query param | Observação |
|---|---|---|
| Código do condomínio | `codCondom` | Retorna unidades vinculadas ao condomínio informado |
| Nome do condômino | `nomeCondomino` | Ignora caixa e acentuação |
| Página | `pagina` | Página mínima: 1 |

Exemplos:

```http
GET /api/unidades?codCondom=5396
GET /api/unidades?nomeCondomino=joao
GET /api/unidades?nomeCondomino=João
GET /api/unidades?codCondom=5396&nomeCondomino=joao
GET /api/unidades?codCondom=5396&pagina=2
```

O tamanho da página é fixo em **10 itens**.

A busca direta de uma unidade é feita por rota, usando `idEconomia`:

```http
GET /api/unidades/{idEconomia}
```

Listagens sem resultado retornam `200 OK` com `itens: []`. A busca direta por `idEconomia` inexistente retorna `404 Not Found`.

---

## Exemplos de resposta

### Condomínios - resposta paginada

```json
{
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1,
  "itens": [
    {
      "codCondom": 5396,
      "nomeCondom": "SOLAR DI TOSCANA",
      "ativo": "S",
      "cnpj": "17474690000113",
      "qtdBlocos": 1,
      "qtdUnidades": 9,
      "totalFracao": 10000000000,
      "dtAlteracao": "2026-07-14T10:00:00",
      "cidade": "Porto Alegre",
      "uf": "RS"
    }
  ]
}
```

### Condomínios - resposta por `codcondom`

```json
{
  "codCondom": 5396,
  "nomeCondom": "SOLAR DI TOSCANA",
  "ativo": "S",
  "cnpj": "17474690000113",
  "qtdBlocos": 1,
  "qtdUnidades": 9,
  "totalFracao": 10000000000,
  "dtAlteracao": "2026-07-14T10:00:00",
  "cidade": "Porto Alegre",
  "uf": "RS"
}
```

### Unidades - resposta paginada

```json
{
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1,
  "itens": [
    {
      "idEconomia": 123,
      "codCondom": 5396,
      "codBloco": "A",
      "codEconom": "101",
      "fracao": 0.08280000,
      "ativa": "S",
      "dataDesativa": "",
      "dtAlteracao": "2026-07-16T00:00:00",
      "tipoUnidade": "Apartamento",
      "codCondomino": "999",
      "nomeCondomino": "João Silva",
      "enderecoPrincipal": "Rua Teste, 123"
    }
  ]
}
```

### Unidades - resposta por `idEconomia`

```json
{
  "idEconomia": 123,
  "codCondom": 5396,
  "codBloco": "A",
  "codEconom": "101",
  "fracao": 0.08280000,
  "ativa": "S",
  "dataDesativa": "",
  "dtAlteracao": "2026-07-16T00:00:00",
  "tipoUnidade": "Apartamento",
  "codCondomino": "999",
  "nomeCondomino": "João Silva",
  "enderecoPrincipal": "Rua Teste, 123"
}
```

---

## Autenticação

Os endpoints da API são protegidos por autenticação JWT Bearer.

Em um fluxo normal de consumo da API, o cliente deve enviar o token no header:

```http
Authorization: Bearer {token}
```

Requisições sem token, com token inválido ou expirado retornam:

```http
401 Unauthorized
```

A validação do JWT é feita pelo pipeline de autenticação do ASP.NET Core.

A API mantém o uso de `[Authorize]` nos endpoints protegidos e valida o token recebido antes de liberar o acesso aos recursos.

---

## Autenticação em ambiente de desenvolvimento

Para facilitar os testes locais durante o desenvolvimento, foi criada uma automação de token restrita ao ambiente `Development`.

Quando a API inicia em modo de desenvolvimento:

1. o `DevTokenStartupService` é executado automaticamente;
2. o `SupabaseDevTokenService` autentica no Supabase Auth usando credenciais configuradas via User Secrets;
3. o token JWT retornado pelo Supabase é armazenado em memória;
4. o token pode ser visualizado pelo terminal ou pelo endpoint `/dev/token`;
5. o `DevTokenInjectionMiddleware` injeta automaticamente o header `Authorization` em requisições para `/api` que chegam sem token.

Com isso, em ambiente local, é possível testar endpoints protegidos pelo Postman, Swagger ou página HTML sem copiar e colar manualmente o Bearer Token.

Essa automação não remove a segurança da API. O `[Authorize]` continua ativo e o JWT continua sendo validado normalmente. A diferença é que, em `Development`, a própria API preenche o header `Authorization` antes da validação.

### Endpoint de apoio `/dev/token`

Em ambiente `Development`, quando habilitado, o endpoint abaixo permite visualizar o token carregado em memória:

```http
GET /dev/token
```

Ele retorna informações como:

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJ...",
  "authorizationHeader": "Bearer eyJ...",
  "expiresAtUtc": "2026-07-10T17:14:21Z"
}
```

Esse endpoint existe apenas para apoio e debug em desenvolvimento.

### Segurança da automação

A automação de token foi limitada ao ambiente `Development`.

Cuidados aplicados:

- o token automático não é registrado em produção;
- o endpoint `/dev/token` só fica disponível em `Development`;
- o middleware só injeta token em rotas `/api`;
- o middleware não sobrescreve um header `Authorization` já informado manualmente;
- as credenciais ficam em User Secrets;
- o token é armazenado apenas em memória;
- a autenticação JWT real permanece ativa.

Em ambientes fora de desenvolvimento, o cliente deve continuar enviando o header `Authorization` normalmente.

---

## Tratamento de erros

A API usa middleware global para padronizar respostas de erro.

| Status | Quando ocorre |
|---|---|
| `200 OK` | Consulta executada com sucesso |
| `400 Bad Request` | Parâmetro inválido |
| `401 Unauthorized` | Token ausente, inválido ou expirado |
| `404 Not Found` | Recurso ou rota não encontrada |
| `500 Internal Server Error` | Erro inesperado |

Exemplo de erro:

```json
{
  "sucesso": false,
  "status": 404,
  "mensagem": "unidade com ideconomia 999999 não foi encontrada.",
  "caminho": "/api/unidades/999999"
}
```

---

## Cache persistente

A API utiliza cache persistente em uma tabela `cache` no PostgreSQL/Supabase.

O cache é aplicado em:

- consulta de condomínio por `codcondom`;
- consulta de condomínios por CNPJ;
- consulta de condomínios por nome;
- consulta de unidade por `idEconomia`;
- consulta de unidades por `codCondom`;
- consulta de unidades por nome do condômino.

Não são cacheadas:

- listagens sem filtro;
- filtros combinados;
- respostas de erro `400`;
- respostas de erro `404`.

Um cache é considerado válido quando:

```sql
chave_cache = chave da consulta
AND expirado_em > now()
AND invalidado_em IS NULL
```

A expiração padrão é de **15 minutos**.

### Tipos de cache utilizados

| Tipo | Quando ocorre | `entidade` | `entidade_id` |
|---|---|---|---|
| `CONDOMINIO_CODCONDOM` | `GET /api/condominios/{codcondom}` | `atlas_condominios` | `codcondom` |
| `CONDOMINIO_NOME` | `GET /api/condominios?nomeCondom=...` | `atlas_condominios` | `null` |
| `CONDOMINIO_CNPJ` | `GET /api/condominios?cnpj=...` | `atlas_condominios` | `null` |
| `UNIDADE_IDECONOMIA` | `GET /api/unidades/{idEconomia}` | `atlas_unidades` | `idEconomia` |
| `UNIDADE_CODCONDOM` | `GET /api/unidades?codCondom=...` | `atlas_unidades` | `null` |
| `UNIDADE_NOME_CONDOMINO` | `GET /api/unidades?nomeCondomino=...` | `atlas_unidades` | `null` |

Nas consultas por nome, CNPJ e `codCondom`, o campo `entidade_id` permanece `null` porque essas consultas seguem o fluxo de listagem e podem representar mais de um registro ou uma consulta filtrada, não uma busca direta por chave única da entidade.

### Estrutura da tabela `cache`

| Campo | Descrição |
|---|---|
| `id` | Identificador único do registro de cache |
| `chave_cache` | Chave única da consulta |
| `url_da_consulta` | URL da consulta realizada |
| `metodo_http` | Método HTTP utilizado |
| `tipo_consulta` | Tipo da consulta cacheada |
| `entidade` | Entidade relacionada ao cache |
| `entidade_id` | ID da entidade, quando aplicável |
| `resposta` | JSON da resposta retornada pela API |
| `status_code` | Status HTTP associado à resposta |
| `criado_em` | Data e hora de criação do cache |
| `expirado_em` | Data e hora de expiração do cache |
| `invalidado_em` | Data e hora de invalidação do cache |
| `motivo_invalidacao` | Motivo da invalidação |

### Invalidação automática

A invalidação ocorre por triggers no banco.

Quando `atlas_condominios` sofre `INSERT`, `UPDATE` ou `DELETE`, os caches de condomínios relacionados são invalidados.

| Operação | Cache invalidado |
|---|---|
| `INSERT` | caches por nome e CNPJ |
| `UPDATE` | cache por `codcondom` do condomínio alterado e caches por nome e CNPJ |
| `DELETE` | cache por `codcondom` do condomínio removido e caches por nome e CNPJ |

Quando `atlas_unidades` sofre `INSERT`, `UPDATE` ou `DELETE`, os caches de unidades relacionados são invalidados.

| Operação | Cache invalidado |
|---|---|
| `INSERT` | caches de listagem por `codCondom` e nome do condômino |
| `UPDATE` | cache por `idEconomia` da unidade alterada e caches de listagem por `codCondom` e nome do condômino |
| `DELETE` | cache por `idEconomia` da unidade removida e caches de listagem por `codCondom` e nome do condômino |

As triggers preenchem os campos `invalidado_em` e `motivo_invalidacao`.

Exemplos de motivo registrado:

```text
Registro da tabela atlas_condominios alterado
Registro da tabela atlas_unidades alterado
```

---

## Banco de dados e scripts Atlas

As migrations são responsáveis pela estrutura base do banco, enquanto os dados de negócio e scripts específicos da carga Atlas ficam separados.

A estrutura Atlas utiliza as tabelas:

```text
public.atlas_condominios
public.atlas_blocos
public.atlas_unidades
```

A API utiliza atualmente:

```text
public.atlas_condominios
public.atlas_unidades
```

Os dados podem ser importados separadamente, por exemplo via CSV no Supabase. Isso mantém o versionamento da estrutura separado da massa de dados.

### Scripts Atlas

Os scripts relacionados à estrutura Atlas ficam em:

```text
database/atlas/sql/
```

Eles contemplam:

- criação das tabelas Atlas;
- validação dos relacionamentos entre condomínios, blocos e unidades;
- atualização da invalidação de cache para `atlas_condominios`;
- atualização da invalidação de cache para `atlas_unidades`.

### Arquivos CSV

Os arquivos CSV de apoio ficam em:

```text
database/atlas/csv/
```

Eles representam a carga de dados utilizada para popular:

- `atlas_condominios`;
- `atlas_blocos`;
- `atlas_unidades`.

---

## Página HTML de consulta

O projeto possui uma página HTML simples para apoiar testes manuais da API.

A tela permite:

- escolher tipo de busca;
- listar dados disponíveis;
- navegar por paginação;
- expandir uma linha para ver detalhes completos;
- visualizar o JSON bruto da resposta;
- alternar modo escuro/claro;
- visualizar ou informar token manualmente, quando necessário.

Em ambiente `Development`, não é necessário preencher manualmente o token para consultar a API, pois o `DevTokenInjectionMiddleware` injeta automaticamente o header `Authorization` nas chamadas para `/api`.

O campo de token foi mantido como apoio visual e para cenários manuais de teste, mas não é obrigatório no fluxo local de desenvolvimento.

Essa página é apenas uma ferramenta de apoio para consulta e validação manual. A lógica principal continua na API.

---

## Swagger

Com a API em execução, acesse:

```text
https://localhost:{porta}/swagger
```

ou, dependendo da configuração local:

```text
http://localhost:{porta}/swagger
```

Pelo Swagger é possível:

- visualizar os endpoints;
- executar requisições;
- conferir parâmetros;
- validar contratos de resposta;
- verificar status codes;
- informar token JWT manualmente, quando necessário.

Em ambiente `Development`, com a automação de token habilitada, é possível executar os endpoints protegidos sem clicar em **Authorize**, pois o middleware injeta automaticamente o header `Authorization` nas chamadas para `/api`.

Em outros ambientes, o uso do Bearer Token manual continua necessário.

---

## Como executar em desenvolvimento

### Pré-requisitos

- .NET 10 SDK
- Git
- Docker, para testes de integração com Testcontainers
- PostgreSQL ou Supabase
- Ferramenta `dotnet-ef`

Instale o `dotnet-ef`, se necessário:

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clonar o repositório

```bash
git clone https://github.com/natemuller/AuxiAPI.git
cd AuxiAPI
```

### 2. Configurar connection string

Use User Secrets para não salvar credenciais no repositório:

```bash
dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "Host=SEU_HOST;Database=postgres;Username=SEU_USUARIO;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true" --project src/AuxiAPI.WebApi.csproj
```

### 3. Configurar credenciais para token automático de desenvolvimento

A automação de token em `Development` utiliza o Supabase Auth para gerar um JWT automaticamente quando a API inicia.

Configure os valores sensíveis via User Secrets:

```bash
dotnet user-secrets set "Supabase:AnonKey" "SUA_ANON_KEY" --project src/AuxiAPI.WebApi.csproj
```

```bash
dotnet user-secrets set "Supabase:Auth:Email" "EMAIL_DO_USUARIO" --project src/AuxiAPI.WebApi.csproj
```

```bash
dotnet user-secrets set "Supabase:Auth:Password" "SENHA_DO_USUARIO" --project src/AuxiAPI.WebApi.csproj
```

O arquivo `appsettings.Development.json` deve conter a configuração do token automático:

```json
"DevToken": {
  "Enabled": true,
  "PrintTokenOnStartup": true,
  "ExposeEndpoint": true,
  "InjectTokenInRequests": true,
  "EndpointPath": "/dev/token",
  "RefreshBeforeExpirationSeconds": 300
}
```

### 4. Restaurar dependências

```bash
dotnet restore
```

### 5. Aplicar migrations

A partir da raiz do projeto:

```bash
dotnet ef database update --project src/AuxiAPI.WebApi.csproj
```

### 6. Criar estrutura Atlas, se necessário

Quando o ambiente ainda não possuir as tabelas Atlas, execute os scripts SQL em:

```text
database/atlas/sql/
```

A ordem esperada é:

```text
01_create_atlas_tables.sql
02_validate_atlas_tables.sql
03_update_cache_invalidation_atlas_condominios.sql
04_update_cache_invalidation_atlas_unidades.sql
```

Depois, importe os CSVs necessários em:

```text
database/atlas/csv/
```

### 7. Compilar e executar

Fluxo utilizado em desenvolvimento:

```bash
cd src
dotnet build
dotnet run
```

Ao iniciar em ambiente `Development`, a aplicação:

- gera automaticamente um token JWT do Supabase;
- armazena o token em memória;
- imprime o token no terminal, se configurado;
- disponibiliza `/dev/token`, se configurado;
- injeta automaticamente o header `Authorization` nas chamadas para `/api` sem token.

Com isso, em desenvolvimento, é possível chamar endpoints protegidos sem informar manualmente o Bearer Token.

---

## Testes

Execute a suíte de testes a partir da raiz do projeto:

```bash
dotnet test
```

Os testes estão organizados por responsabilidade:

| Pasta | O que valida |
|---|---|
| `ControllersTest` | Comportamento da camada controller |
| `DTOsTest` | Normalizações, queries e mapeamentos para DTO |
| `MiddlewaresTest` | Tratamento global de exceções e injeção automática de token em Development |
| `ServicesTest` | Regras de service, cache, paginação e validações |
| `RepositoriesTest` | Consultas, filtros, paginação e cache repository |
| `IntegrationTest` | Endpoints, autenticação, erros e triggers de cache |
| `TestInfrastructure` | Base para autenticação fake e PostgreSQL em container para testes |

Para os testes de integração, mantenha o Docker em execução.

### Cobertura atual validada

A suíte cobre os principais comportamentos da API:

- filtros de condomínios por CNPJ e nome;
- filtros de unidades por `codCondom` e nome do condômino;
- paginação;
- busca por `codcondom`;
- busca por `idEconomia`;
- autenticação;
- tratamento global de erros;
- cache persistente;
- cache de condomínios por `codcondom`, CNPJ e nome;
- cache de unidades por `idEconomia`, `codCondom` e nome do condômino;
- invalidação automática por trigger em `atlas_condominios`;
- invalidação automática por trigger em `atlas_unidades`;
- repository de cache;
- service de cache;
- endpoint de Condomínios;
- endpoint de Unidades;
- middleware de injeção automática de token em Development;
- contrato Atlas dos endpoints implementados.

Resultado esperado da validação:

```text
dotnet build
dotnet test
```

---

## Participantes do desafio

- **Desenvolvedor:** Natan Müller
- **Mentor:** Marcel Guinther
- **Gestor apoiador:** Alexandre Cambraia
- **Tech Lead:** Rodrigo Silva