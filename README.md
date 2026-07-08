# AuxiAPI

API REST desenvolvida em **.NET 10** para consulta de dados do domínio condominial, criada como parte do desafio **StartAPI - Da Lógica à Prática na Criação de API**.

O objetivo do projeto é consolidar conhecimentos práticos de desenvolvimento back-end, aplicando conceitos de API REST, HTTP, JSON, separação de responsabilidades, tratamento de erros, testes, cache persistente e documentação.

---

## Contexto do desafio

O desafio propõe o desenvolvimento de endpoints de consulta para mapear o fluxo estrutural de dados do domínio condominial:

1. **Condomínios**
2. **Torres**
3. **Unidades**

Nesta etapa, o foco implementado está no endpoint de **Condomínios**, responsável pela listagem e consulta das informações gerais dos condomínios.

---

## Escopo atual

Implementado nesta etapa:

- endpoint de Condomínios;
- listagem paginada;
- busca por ID;
- filtros por query params;
- validação de entrada;
- tratamento padronizado de erros;
- autenticação JWT;
- cache persistente em tabela no Supabase/PostgreSQL;
- invalidação automática de cache via trigger no banco;
- documentação com Swagger;
- testes automatizados.

Fora do escopo atual:

- endpoint de Torres;
- endpoint de Unidades;
- operações de criação, edição ou exclusão;
- regras de negócio relacionadas a cadastro.

---

## Tecnologias utilizadas

- .NET 10
- C#
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Supabase
- JWT Bearer Authentication
- Swagger / OpenAPI
- xUnit
- Moq
- Testcontainers
- Docker

---

## Arquitetura do projeto

O projeto utiliza uma arquitetura em camadas, separando as responsabilidades principais da aplicação.

```text
AuxiAPI/
├── src/
│   ├── Common/
│   │   ├── Text/
│   │   └── MensagensDeErro.cs
│   │
│   ├── Contexts/
│   │   ├── CondominiosDbContext.cs
│   │   └── PostgresDbFunctions.cs
│   │
│   ├── Controllers/
│   │   └── CondominiosController.cs
│   │
│   ├── DTOs/
│   │   ├── InformacoesCondominioDto.cs
│   │   ├── ResultadoPaginadoDto.cs
│   │   └── VisualizarCondominioQuery.cs
│   │
│   ├── Entities/
│   │   ├── Condominio.cs
│   │   └── CacheEntry.cs
│   │
│   ├── Middlewares/
│   │   ├── GlobalExceptionHandler.cs
│   │   └── ValidarModelStateFilter.cs
│   │
│   ├── Migrations/
│   │
│   ├── Properties/
│   │
│   ├── Repositories/
│   │   ├── ICondominioRepository.cs
│   │   ├── CondominioRepository.cs
│   │   ├── ICacheRepository.cs
│   │   └── CacheRepository.cs
│   │
│   ├── Services/
│   │   ├── CondominioService.cs
│   │   ├── IDatabaseCacheService.cs
│   │   └── DatabaseCacheService.cs
│   │
│   ├── wwwroot/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── AuxiAPI.WebApi.csproj
│   ├── AuxiAPI.WebApi.http
│   └── Program.cs
│
├── test/
│   ├── ControllersTest/
│   ├── DTOsTest/
│   ├── IntegrationTest/
│   ├── MiddlewaresTest/
│   ├── RepositoriesTest/
│   ├── ServicesTest/
│   ├── TestInfrastructure/
│   ├── AssemblyInfo.cs
│   └── AuxiAPI.Tests.csproj
│
├── .gitignore
├── AuxiAPI.sln
├── cls
└── README.md
```

> As pastas `bin/`, `obj/` e `TestResults/` são geradas por build/teste e não fazem parte da arquitetura lógica do projeto.

---

## Responsabilidades das camadas

### Controller

A camada de controller é responsável por receber as requisições HTTP, chamar os serviços necessários e retornar a resposta para o cliente.

No endpoint de Condomínios, o controller expõe:

```http
GET /api/condominios
GET /api/condominios/{id}
```

O controller não concentra regra de negócio. Ele atua apenas como entrada da API.

---

### Service

A camada de service concentra as regras da aplicação.

No endpoint de Condomínios, o service é responsável por:

- validar parâmetros recebidos;
- normalizar filtros;
- aplicar regras de paginação;
- consultar o cache persistente antes de acessar a tabela de condomínios;
- salvar novas respostas na tabela `cache` quando não houver cache válido;
- aplicar cache nas consultas por ID e por nome do condomínio;
- converter entidades para DTOs;
- tratar cenários de dados não encontrados.

---

### Repository

A camada de repository é responsável pelo acesso ao banco de dados usando Entity Framework Core.

No endpoint de Condomínios, o repository executa:

- busca por ID;
- listagem paginada;
- filtros por código do condomínio;
- filtros por CNPJ;
- filtros por nome;
- consultas otimizadas para leitura com `AsNoTracking()`.

---

### Cache Repository

A camada de cache repository é responsável pelo acesso à tabela `cache`.

Ela executa:

- busca de cache válido por chave;
- verificação de expiração;
- verificação de invalidação;
- persistência de novas respostas cacheadas.

Um registro de cache só é considerado válido quando:

- a chave da consulta é a mesma;
- `expirado_em` é maior que a data/hora atual;
- `invalidado_em` está nulo.

---

### DTOs

Os DTOs definem os contratos de entrada e saída da API.

A API não retorna diretamente a entidade do banco de dados, evitando acoplamento entre a estrutura interna da aplicação e o contrato exposto ao cliente.

---

## Endpoint de Condomínios

### Listar condomínios

```http
GET /api/condominios
```

Retorna uma lista paginada de condomínios.

---

### Buscar condomínio por ID

```http
GET /api/condominios/{id}
```

Exemplo:

```http
GET /api/condominios/1
```

Retorna as informações de um condomínio específico.

---

### Filtros disponíveis

O endpoint de listagem aceita filtros via query params.

#### Filtrar por código do condomínio

```http
GET /api/condominios?CodigoDoCondominio=0001
```

O código do condomínio possui regra de quatro dígitos. Pesquisas como `1` são interpretadas como `0001`.

```http
GET /api/condominios?CodigoDoCondominio=1
```

---

#### Filtrar por CNPJ

```http
GET /api/condominios?CNPJDoCondominio=12345678000101
```

O CNPJ pode ser informado com ou sem máscara.

```http
GET /api/condominios?CNPJDoCondominio=12.345.678/0001-01
```

---

#### Filtrar por nome

```http
GET /api/condominios?NomeDoCondominio=Residencial
```

A busca por nome ignora diferenças de maiúsculas, minúsculas e acentuação.

---

#### Paginação

```http
GET /api/condominios?Pagina=2
```

O tamanho da página é fixo em 10 itens.

---

#### Combinação de filtros

```http
GET /api/condominios?NomeDoCondominio=Residencial&Pagina=1
```

---

## Exemplo de resposta paginada

```json
{
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1,
  "itens": [
    {
      "codigoDoCondominio": "0001",
      "cnpjDoCondominio": "12345678000101",
      "nomeDoCondominio": "Residencial Exemplo",
      "endereco": "Rua Exemplo",
      "numeroDoEndereco": "123",
      "estadoDoEndereco": "RS",
      "cidadeDoEndereco": "Porto Alegre",
      "bairroDoEndereco": "Centro",
      "cepDoEndereco": "90000000",
      "numeroDeTorres": 2,
      "numeroDeUnidades": 120,
      "status": "Ativo",
      "dataInicial_Administracao": "2024-01-01",
      "dataFinal_Administracao": "",
      "nomeGerenteDeContas": "Nome do Gerente",
      "nomeSindico": "Nome do Síndico"
    }
  ]
}
```

---

## Exemplo de resposta por ID

```json
{
  "codigoDoCondominio": "0001",
  "cnpjDoCondominio": "12345678000101",
  "nomeDoCondominio": "Residencial Exemplo",
  "endereco": "Rua Exemplo",
  "numeroDoEndereco": "123",
  "estadoDoEndereco": "RS",
  "cidadeDoEndereco": "Porto Alegre",
  "bairroDoEndereco": "Centro",
  "cepDoEndereco": "90000000",
  "numeroDeTorres": 2,
  "numeroDeUnidades": 120,
  "status": "Ativo",
  "dataInicial_Administracao": "2024-01-01",
  "dataFinal_Administracao": "",
  "nomeGerenteDeContas": "Nome do Gerente",
  "nomeSindico": "Nome do Síndico"
}
```

---

## Status codes

A API utiliza status codes HTTP para representar corretamente o resultado das requisições.

| Status | Significado |
| --- | --- |
| `200 OK` | Requisição executada com sucesso |
| `400 Bad Request` | Parâmetros inválidos |
| `401 Unauthorized` | Token JWT ausente, inválido ou expirado |
| `404 Not Found` | Condomínio ou rota não encontrada |
| `500 Internal Server Error` | Erro inesperado no servidor |

---

## Tratamento de erros

A API possui um middleware global para padronizar respostas de erro.

### Exemplo de erro para rota inexistente

```json
{
  "sucesso": false,
  "status": 404,
  "mensagem": "a rota ou endpoint solicitado nao existe."
}
```

### Exemplo de erro para parâmetro inválido

```json
{
  "sucesso": false,
  "status": 400,
  "mensagem": "pagina deve ser maior ou igual a 1."
}
```

### Exemplo de erro para condomínio não encontrado

```json
{
  "sucesso": false,
  "status": 404,
  "mensagem": "condomínio com id 9999 não foi encontrado.",
  "caminho": "/api/condominios/9999"
}
```

---

## Autenticação

O endpoint de Condomínios é protegido por autenticação JWT.

Para consumir a API, é necessário informar um token válido no header da requisição.

```http
Authorization: Bearer {token}
```

No Swagger, clique em **Authorize** e informe o token no formato:

```text
Bearer {token}
```

Requisições sem token ou com token inválido retornam:

```http
401 Unauthorized
```

---

## Cache persistente

A API utiliza cache persistente em uma tabela `cache` no PostgreSQL/Supabase.

O cache é aplicado nas consultas de:

- condomínio por ID;
- condomínios por nome.

Consultas com outros filtros, como código do condomínio ou CNPJ, continuam sendo executadas diretamente na tabela `condominios`.

Um registro de cache é considerado válido quando:

```sql
chave_cache = chave da consulta
AND expirado_em > now()
AND invalidado_em IS NULL
```

Quando não existe cache válido, a API consulta a tabela `condominios`, retorna a resposta e cria um novo registro na tabela `cache`.

Os registros possuem expiração fixa de 15 minutos.

---

### Estrutura da tabela `cache`

| Campo | Descrição |
| --- | --- |
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

---

### Invalidação automática

A invalidação do cache é feita por trigger no PostgreSQL/Supabase.

Quando ocorre `INSERT`, `UPDATE` ou `DELETE` na tabela `condominios`, a trigger invalida os caches relacionados.

A regra aplicada é:

- `UPDATE`: invalida o cache por ID do condomínio alterado e todos os caches por nome;
- `DELETE`: invalida o cache por ID do condomínio removido e todos os caches por nome;
- `INSERT`: invalida todos os caches por nome.

A invalidação preenche os campos `invalidado_em` e `motivo_invalidacao`.

---

## Swagger

A API possui documentação interativa com Swagger/OpenAPI.

Com a aplicação em execução, acesse:

```text
https://localhost:{porta}/swagger
```

Pelo Swagger é possível:

- visualizar os endpoints disponíveis;
- informar o token JWT;
- executar requisições de teste;
- verificar contratos de entrada e saída;
- validar status codes retornados pela API.

---

## Pré-requisitos

Antes de executar o projeto, instale:

- .NET 10 SDK;
- Git;
- Docker, caso deseje executar testes de integração com Testcontainers;
- acesso a um banco PostgreSQL ou Supabase;
- ferramenta `dotnet-ef`, caso ainda não esteja instalada.

Para instalar a ferramenta do Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

---

## Como executar o projeto

### 1. Clonar o repositório

```bash
git clone https://github.com/natemuller/AuxiAPI.git
cd AuxiAPI
```

---

### 2. Configurar a connection string

Use User Secrets para evitar salvar credenciais no repositório.

Execute o comando abaixo ajustando os dados do banco:

```bash
dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "Host=SEU_HOST;Database=postgres;Username=SEU_USUARIO;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true" --project src/AuxiAPI.WebApi.csproj
```

---

### 3. Restaurar dependências

```bash
dotnet restore
```

---

### 4. Aplicar migrations no banco

Antes de executar a API em um banco novo, aplique as migrations:

```bash
dotnet ef database update --project src/AuxiAPI.WebApi.csproj
```

As migrations criam a estrutura necessária no banco, incluindo a tabela `cache` e a trigger de invalidação automática.

---

### 5. Compilar o projeto

```bash
dotnet build
```

---

### 6. Executar a API

```bash
dotnet run --project src/AuxiAPI.WebApi.csproj
```

---

### 7. Acessar o Swagger

Com a aplicação rodando, acesse:

```text
https://localhost:{porta}/swagger
```

---

## Como rodar os testes

Execute:

```bash
dotnet test
```

Para os testes de integração, mantenha o Docker aberto, pois eles podem depender de containers de banco de dados.

---

## Exemplos de requisições com curl

### Listar condomínios

```bash
curl -X GET "https://localhost:{porta}/api/condominios" \
  -H "Authorization: Bearer {token}"
```

---

### Buscar por ID

```bash
curl -X GET "https://localhost:{porta}/api/condominios/1" \
  -H "Authorization: Bearer {token}"
```

---

### Filtrar por nome

```bash
curl -X GET "https://localhost:{porta}/api/condominios?NomeDoCondominio=Residencial" \
  -H "Authorization: Bearer {token}"
```

---

### Filtrar por CNPJ

```bash
curl -X GET "https://localhost:{porta}/api/condominios?CNPJDoCondominio=12345678000101" \
  -H "Authorization: Bearer {token}"
```

---

### Filtrar por código do condomínio

```bash
curl -X GET "https://localhost:{porta}/api/condominios?CodigoDoCondominio=0001" \
  -H "Authorization: Bearer {token}"
```

---

## Organização dos testes

Os testes estão separados por responsabilidade.

```text
test/
├── ControllersTest/
├── DTOsTest/
├── IntegrationTest/
├── MiddlewaresTest/
├── RepositoriesTest/
├── ServicesTest/
└── TestInfrastructure/
```

### ControllersTest

Testa a camada de controller, validando se as requisições HTTP retornam os status codes e objetos esperados.

### DTOsTest

Testa regras específicas dos DTOs de entrada, como normalização de CNPJ.

### MiddlewaresTest

Testa os middlewares da aplicação, principalmente o tratamento global de exceções.

### ServicesTest

Testa as regras da camada de service, incluindo validações, paginação, mapeamento de entidade para DTO, cache por ID, cache por nome e tratamento de dados não encontrados.

### RepositoriesTest

Testa o acesso a dados, filtros, paginação, busca por ID e regras de leitura/escrita da tabela de cache.

### IntegrationTest

Testa o comportamento da API de forma integrada, incluindo autenticação, endpoints, tratamento de erros e invalidação automática do cache via trigger.

### TestInfrastructure

Contém estruturas auxiliares para testes, como autenticação fake, autenticação inválida e fixture de PostgreSQL com Testcontainers.

---

## Boas práticas aplicadas

O projeto aplica boas práticas importantes para APIs REST:

- separação de responsabilidades;
- uso de DTOs;
- uso de repository pattern;
- service layer;
- tratamento global de exceções;
- validação de model state;
- status codes adequados;
- documentação com Swagger;
- autenticação com JWT;
- cache persistente em banco de dados;
- invalidação automática de cache via trigger;
- consultas de leitura com `AsNoTracking()`;
- testes automatizados;
- testes de integração com banco em container.

---

## Decisões técnicas

### Por que usar DTOs?

DTOs evitam expor diretamente a entidade do banco de dados para o cliente da API.

Isso permite mudar a estrutura interna da aplicação sem quebrar o contrato público da API.

---

### Por que separar Controller, Service e Repository?

A separação melhora a organização e facilita manutenção, testes e evolução do projeto.

Cada camada possui uma responsabilidade clara:

- Controller: entrada HTTP;
- Service: regras da aplicação;
- Repository: acesso ao banco de dados.

---

### Por que usar paginação?

A paginação evita retornar grandes volumes de dados em uma única resposta.

Isso melhora a performance da API e reduz o custo de tráfego entre cliente e servidor.

---

### Por que usar cache persistente?

O cache reduz consultas repetidas ao banco de dados em cenários de leitura frequente.

Neste projeto, o cache foi implementado em uma tabela no PostgreSQL/Supabase para permitir:

- visualizar os registros cacheados;
- controlar data de criação e expiração;
- invalidar registros quando os dados originais forem alterados;
- testar o comportamento do cache de forma mais clara.

A abordagem foi aplicada ao endpoint de Condomínios nas consultas por ID e por nome.

---

### Por que usar trigger para invalidação?

Como o cache fica salvo em uma tabela do banco, a invalidação precisa acompanhar alterações feitas na tabela de origem.

A trigger garante que, quando a tabela `condominios` for alterada, os caches relacionados sejam invalidados mesmo que a alteração não tenha sido feita diretamente pela API.

Isso evita que a aplicação retorne dados antigos após alterações no banco.

---

### Por que usar Swagger?

O Swagger facilita a documentação e o teste manual da API.

Ele permite que outros desenvolvedores entendam rapidamente quais endpoints existem, quais parâmetros são aceitos e quais respostas são esperadas.

---

## Convenções de retorno

A API retorna dados em formato JSON.

Exemplo de sucesso por ID:

```json
{
  "codigoDoCondominio": "0001",
  "cnpjDoCondominio": "12345678000101",
  "nomeDoCondominio": "Residencial Exemplo"
}
```

Exemplo de sucesso paginado:

```json
{
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1,
  "itens": [
    {
      "codigoDoCondominio": "0001",
      "cnpjDoCondominio": "12345678000101",
      "nomeDoCondominio": "Residencial Exemplo"
    }
  ]
}
```

Exemplo de erro:

```json
{
  "sucesso": false,
  "status": 404,
  "mensagem": "condomínio com id 9999 não foi encontrado.",
  "caminho": "/api/condominios/9999"
}
```

---

## Participantes do desafio

- **Desenvolvedor:** Natan Müller
- **Mentor:** Marcel Guinther
- **Gestor apoiador:** Alexandre Cambraia
- **Tech Lead:** Rodrigo Silva
- **Área:** Tecnologia da Informação