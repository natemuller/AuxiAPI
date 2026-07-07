````md
# AuxiAPI

API REST desenvolvida em **.NET 10** para consulta de dados do domínio condominial, criada como parte do desafio **StartAPI - Da Lógica à Prática na Criação de API**.

O objetivo do projeto é consolidar conhecimentos práticos de desenvolvimento back-end, aplicando conceitos de API REST, HTTP, JSON, separação de responsabilidades, tratamento de erros, testes e documentação.

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
- cache em memória;
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
│   │   ├── Cache/
│   │   └── MensagensDeErro.cs
│   │
│   ├── Contexts/
│   │   └── CondominiosDbContext.cs
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
│   │   └── Condominio.cs
│   │
│   ├── Middlewares/
│   │   ├── GlobalExceptionHandler.cs
│   │   └── ValidarModelStateFilter.cs
│   │
│   ├── Repositories/
│   │   ├── ICondominioRepository.cs
│   │   └── CondominioRepository.cs
│   │
│   ├── Services/
│   │   └── CondominioService.cs
│   │
│   ├── Program.cs
│   └── AuxiAPI.WebApi.csproj
│
├── test/
│   ├── ControllersTest/
│   ├── ServicesTest/
│   ├── RepositoriesTest/
│   ├── IntegrationTest/
│   └── TestInfrastructure/
│
└── README.md
```

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
- consultar dados através do repository;
- aplicar cache quando necessário;
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

#### Filtrar por CNPJ

```http
GET /api/condominios?CNPJDoCondominio=12345678000101
```

#### Filtrar por nome

```http
GET /api/condominios?NomeDoCondominio=Residencial
```

#### Paginação

```http
GET /api/condominios?Pagina=2
```

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
      "dataInicial_Administracao": "2024-01-01T00:00:00",
      "dataFinal_Administracao": null,
      "nomeGerenteDeContas": "Nome do Gerente",
      "nomeSindico": "Nome do Síndico"
    }
  ]
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
  "mensagem": "condominio nao encontrado."
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

## Cache

A API utiliza cache em memória com `IMemoryCache`.

O cache foi aplicado na camada de service para reduzir consultas repetidas ao banco de dados e manter o controller simples.

Atualmente, o cache é utilizado em consultas como:

- busca por ID;
- busca por nome do condomínio.

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
- acesso a um banco PostgreSQL ou Supabase.

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

### 4. Compilar o projeto

```bash
dotnet build
```

---

### 5. Executar a API

```bash
dotnet run --project src/AuxiAPI.WebApi.csproj
```

---

### 6. Acessar o Swagger

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
├── ServicesTest/
├── RepositoriesTest/
├── IntegrationTest/
└── TestInfrastructure/
```

### ControllersTest

Testa a camada de controller, validando se as requisições HTTP retornam os status codes e objetos esperados.

### ServicesTest

Testa as regras da camada de service, incluindo validações, paginação, cache e tratamento de dados não encontrados.

### RepositoriesTest

Testa o acesso a dados, filtros e consultas realizadas pelo repository.

### IntegrationTest

Testa o comportamento da API de forma integrada, simulando chamadas reais aos endpoints.

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
- cache em memória;
- consultas de leitura com `AsNoTracking()`;
- testes automatizados.

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

### Por que usar cache?

O cache reduz consultas repetidas ao banco de dados em cenários de leitura frequente.

Como o endpoint de Condomínios é um endpoint de consulta, o uso de cache ajuda a melhorar o tempo de resposta.

---

### Por que usar Swagger?

O Swagger facilita a documentação e o teste manual da API.

Ele permite que outros desenvolvedores entendam rapidamente quais endpoints existem, quais parâmetros são aceitos e quais respostas são esperadas.

---

## Convenções de retorno

A API retorna dados em formato JSON.

Exemplo de sucesso:

```json
{
  "codigoDoCondominio": "0001",
  "cnpjDoCondominio": "12345678000101",
  "nomeDoCondominio": "Residencial Exemplo"
}
```

Exemplo de erro:

```json
{
  "sucesso": false,
  "status": 404,
  "mensagem": "condominio nao encontrado."
}
```

---

## Participantes do desafio

- **Desenvolvedor:** Natan Müller
- **Mentor:** Marcel Guinther
- **Gestor apoiador:** Alexandre Cambraia
- **Tech Lead:** Rodrigo Silva
- **Área:** Tecnologia da Informação

