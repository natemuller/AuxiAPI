# AuxiAPI

API REST em **.NET 10** para consulta de dados do domínio condominial, desenvolvida no desafio **StartAPI - Da Lógica à Prática na Criação de API**.

O projeto tem como objetivo praticar a construção de uma API REST realista, aplicando conceitos de HTTP, JSON, autenticação, separação de responsabilidades, acesso a banco de dados, tratamento de erros, cache persistente, documentação e testes automatizados.

Nesta etapa, o foco implementado é o endpoint de **Condomínios**. Os endpoints de **Torres** e **Unidades** ficam como evolução futura.

---

## Status do projeto

| Área | Status |
|---|---|
| Endpoint de Condomínios | Implementado |
| Listagem paginada | Implementada |
| Busca por ID | Implementada |
| Filtros por código, CNPJ e nome | Implementados |
| Autenticação JWT | Implementada |
| Tratamento global de erros | Implementado |
| Cache persistente em PostgreSQL/Supabase | Implementado |
| Invalidação automática de cache por trigger | Implementada |
| Swagger/OpenAPI | Implementado |
| Página HTML de consulta | Implementada |
| Testes automatizados | Implementados |
| Endpoints de Torres e Unidades | Futuro |

---

## Contexto do desafio

O desafio propõe o desenvolvimento de endpoints de consulta para mapear o fluxo estrutural de dados do domínio condominial:

1. **Condomínios**
2. **Torres**
3. **Unidades**

O endpoint de **Condomínios** foi priorizado nesta fase para consolidar a base técnica da API antes da evolução para os demais recursos.

---

## Escopo atual

O escopo atual cobre a consulta de condomínios por meio dos endpoints:

    GET /api/condominios
    GET /api/condominios/{id}

A API permite:

- listar condomínios com paginação;
- consultar um condomínio por ID;
- filtrar por código do condomínio;
- filtrar por CNPJ com ou sem máscara;
- filtrar por nome ignorando diferenças de caixa e acentuação;
- retornar erros padronizados;
- proteger os endpoints com JWT;
- cachear consultas por ID e por nome;
- invalidar cache automaticamente quando a tabela `condominios` é alterada;
- consultar manualmente os dados por uma página HTML de apoio.

Fora do escopo atual:

- criação, edição ou exclusão de condomínios;
- endpoint de Torres;
- endpoint de Unidades;
- regras completas do banco real definitivo;
- modelagem final envolvendo múltiplas tabelas.

---

## Tecnologias utilizadas

- **.NET 10**
- **C#**
- **ASP.NET Core**
- **Entity Framework Core**
- **PostgreSQL**
- **Supabase**
- **JWT Bearer Authentication**
- **Swagger / OpenAPI**
- **xUnit**
- **Moq**
- **Testcontainers**
- **Docker**
- **HTML, CSS e JavaScript** para a página simples de consulta

---

## Arquitetura

O projeto usa uma organização em camadas para separar responsabilidades e facilitar manutenção, testes e evolução.

    AuxiAPI/
    ├── src/
    │   ├── Common/
    │   ├── Contexts/
    │   │   ├── Configurations/
    │   │   ├── CondominiosDbContext.cs
    │   │   └── PostgresDbFunctions.cs
    │   ├── Controllers/
    │   ├── DTOs/
    │   ├── Entities/
    │   ├── Middlewares/
    │   ├── Migrations/
    │   ├── Repositories/
    │   ├── Services/
    │   ├── wwwroot/
    │   └── Program.cs
    │
    ├── test/
    │   ├── ControllersTest/
    │   ├── DTOsTest/
    │   ├── IntegrationTest/
    │   ├── MiddlewaresTest/
    │   ├── RepositoriesTest/
    │   ├── ServicesTest/
    │   └── TestInfrastructure/
    │
    ├── AuxiAPI.sln
    └── README.md

As pastas `bin/`, `obj/` e `TestResults/` são geradas por build/teste e não fazem parte da arquitetura lógica do projeto.

---

## Responsabilidades das camadas

| Camada | Responsabilidade |
|---|---|
| Controller | Recebe requisições HTTP e retorna respostas |
| Service | Aplica validações, regras de paginação, cache e mapeamento para DTO |
| Repository | Consulta o banco com Entity Framework Core |
| DTOs | Definem contratos de entrada e saída da API |
| Middleware | Padroniza tratamento de erros |
| CacheRepository | Lê e grava registros na tabela de cache |
| TestInfrastructure | Fornece autenticação fake e banco PostgreSQL em container para testes |

### Controller

A camada de controller expõe os endpoints HTTP da API.

No endpoint de Condomínios, o controller disponibiliza:

    GET /api/condominios
    GET /api/condominios/{id}

O controller não concentra regra de negócio. Ele recebe a requisição, chama o service e retorna a resposta adequada.

### Service

A camada de service concentra as regras da aplicação.

No endpoint de Condomínios, ela é responsável por:

- validar parâmetros recebidos;
- aplicar regra de paginação;
- consultar cache persistente;
- salvar respostas cacheadas quando aplicável;
- mapear entidade para DTO;
- tratar cenário de condomínio não encontrado.

### Repository

A camada de repository acessa o banco de dados com Entity Framework Core.

Ela executa:

- busca por ID;
- listagem paginada;
- filtro por código;
- filtro por CNPJ;
- filtro por nome;
- consultas de leitura usando `AsNoTracking()`.

### DTOs

Os DTOs definem os contratos de entrada e saída da API.

A API não retorna diretamente a entidade do banco. Isso evita acoplamento entre a estrutura interna da aplicação e o contrato público exposto para o cliente.

---

## Endpoint de Condomínios

### Listar condomínios

    GET /api/condominios

Retorna uma lista paginada de condomínios.

Exemplo:

    GET /api/condominios?Pagina=1

### Buscar condomínio por ID

    GET /api/condominios/{id}

Exemplo:

    GET /api/condominios/1

### Filtros disponíveis

| Filtro | Query param | Observação |
|---|---|---|
| Código | `CodigoDoCondominio` | Aceita `1` ou `0001` |
| CNPJ | `CNPJDoCondominio` | Aceita com ou sem máscara |
| Nome | `NomeDoCondominio` | Ignora caixa e acentuação |
| Página | `Pagina` | Página mínima: 1 |

Exemplos:

    GET /api/condominios?CodigoDoCondominio=1
    GET /api/condominios?CodigoDoCondominio=0001
    GET /api/condominios?CNPJDoCondominio=12345678000101
    GET /api/condominios?CNPJDoCondominio=12.345.678/0001-01
    GET /api/condominios?NomeDoCondominio=Residencial
    GET /api/condominios?NomeDoCondominio=Residencial&Pagina=2

O tamanho da página é fixo em **10 itens**.

---

## Exemplos de resposta

### Resposta paginada

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

### Resposta por ID

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

---

## Autenticação

Os endpoints de Condomínios exigem autenticação JWT.

Use o header:

    Authorization: Bearer {token}

No Swagger, clique em **Authorize** e informe:

    Bearer {token}

Requisições sem token, com token inválido ou expirado retornam:

    401 Unauthorized

---

## Tratamento de erros

A API usa middleware global para padronizar respostas de erro.

| Status | Quando ocorre |
|---|---|
| `200 OK` | Consulta executada com sucesso |
| `400 Bad Request` | Parâmetro inválido |
| `401 Unauthorized` | Token ausente, inválido ou expirado |
| `404 Not Found` | Condomínio ou rota não encontrada |
| `500 Internal Server Error` | Erro inesperado |

Exemplo de erro:

    {
      "sucesso": false,
      "status": 404,
      "mensagem": "condomínio com id 9999 não foi encontrado.",
      "caminho": "/api/condominios/9999"
    }

---

## Cache persistente

A API utiliza cache persistente em uma tabela `cache` no PostgreSQL/Supabase.

O cache é aplicado em:

- consulta de condomínio por ID;
- consulta de condomínios por nome.

Não são cacheadas:

- consultas por código;
- consultas por CNPJ;
- filtros combinados com código ou CNPJ;
- respostas de erro `400`;
- respostas de erro `404`.

Um cache é considerado válido quando:

    chave_cache = chave da consulta
    AND expirado_em > now()
    AND invalidado_em IS NULL

A expiração padrão é de **15 minutos**.

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

A invalidação ocorre por trigger no banco.

Quando a tabela `condominios` sofre `INSERT`, `UPDATE` ou `DELETE`, os caches relacionados são invalidados.

| Operação | Cache invalidado |
|---|---|
| `INSERT` | caches por nome |
| `UPDATE` | cache por ID do condomínio alterado e caches por nome |
| `DELETE` | cache por ID do condomínio removido e caches por nome |

A trigger preenche os campos `invalidado_em` e `motivo_invalidacao`.

---

## Banco de dados e migrations

As migrations são responsáveis pela estrutura do banco, não pela carga de dados de negócio.

Elas criam:

- tabela `condominios`;
- tabela `cache`;
- índices da tabela de cache;
- extensão/função necessária para busca sem acento;
- trigger de invalidação automática de cache.

Os dados de condomínios podem ser importados separadamente, por exemplo via CSV no Supabase. Isso mantém o versionamento da estrutura separado da massa de dados.

---

## Página HTML de consulta

O projeto possui uma página HTML simples para apoiar testes manuais do endpoint de Condomínios.

A tela permite:

- informar token JWT;
- escolher tipo de busca;
- listar condomínios;
- buscar por ID, código, CNPJ ou nome;
- navegar por paginação;
- expandir uma linha para ver detalhes completos;
- visualizar o JSON bruto da resposta;
- alternar modo escuro/claro.

Essa página é apenas uma ferramenta de apoio para consulta e validação manual. A lógica principal continua na API.

---

## Swagger

Com a API em execução, acesse:

    https://localhost:{porta}/swagger

Pelo Swagger é possível:

- visualizar os endpoints;
- informar token JWT;
- executar requisições;
- conferir parâmetros;
- validar contratos de resposta;
- verificar status codes.

---

## Como executar

### Pré-requisitos

- .NET 10 SDK
- Git
- Docker, para testes de integração com Testcontainers
- PostgreSQL ou Supabase
- Ferramenta `dotnet-ef`

Instale o `dotnet-ef`, se necessário:

    dotnet tool install --global dotnet-ef

### 1. Clonar o repositório

    git clone https://github.com/natemuller/AuxiAPI.git
    cd AuxiAPI

### 2. Configurar connection string

Use User Secrets para não salvar credenciais no repositório:

    dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "Host=SEU_HOST;Database=postgres;Username=SEU_USUARIO;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true" --project src/AuxiAPI.WebApi.csproj

### 3. Restaurar dependências

    dotnet restore

### 4. Aplicar migrations

    dotnet ef database update --project src/AuxiAPI.WebApi.csproj

### 5. Compilar

    dotnet build

### 6. Executar

    dotnet run --project src/AuxiAPI.WebApi.csproj

---

## Testes

Execute:

    dotnet test

Os testes estão organizados por responsabilidade:

| Pasta | O que valida |
|---|---|
| `ControllersTest` | Comportamento da camada controller |
| `DTOsTest` | Normalizações e regras dos DTOs |
| `MiddlewaresTest` | Tratamento global de exceções |
| `ServicesTest` | Regras de service, cache, paginação e validações |
| `RepositoriesTest` | Consultas, filtros, paginação e cache repository |
| `IntegrationTest` | Endpoints, autenticação, erros e trigger de cache |
| `TestInfrastructure` | Base para autenticação fake e PostgreSQL em container |

Para os testes de integração, mantenha o Docker em execução.

---

## Exemplos com curl

### Listar condomínios

    curl -X GET "https://localhost:{porta}/api/condominios" \
      -H "Authorization: Bearer {token}"

### Buscar por ID

    curl -X GET "https://localhost:{porta}/api/condominios/1" \
      -H "Authorization: Bearer {token}"

### Filtrar por nome

    curl -X GET "https://localhost:{porta}/api/condominios?NomeDoCondominio=Residencial" \
      -H "Authorization: Bearer {token}"

### Filtrar por CNPJ

    curl -X GET "https://localhost:{porta}/api/condominios?CNPJDoCondominio=12.345.678/0001-01" \
      -H "Authorization: Bearer {token}"

### Filtrar por código

    curl -X GET "https://localhost:{porta}/api/condominios?CodigoDoCondominio=1" \
      -H "Authorization: Bearer {token}"

---

## Boas práticas aplicadas

- separação entre Controller, Service e Repository;
- uso de DTOs para contrato de API;
- tratamento global de exceções;
- validação de entrada;
- autenticação JWT;
- documentação com Swagger;
- consultas de leitura com `AsNoTracking()`;
- paginação;
- cache persistente em banco;
- invalidação automática de cache;
- migrations sem seed de dados de negócio;
- testes unitários e de integração.

---

## Decisões técnicas

### Por que usar DTOs?

DTOs evitam expor diretamente a entidade do banco de dados para o cliente da API.

Isso permite mudar a estrutura interna da aplicação sem quebrar o contrato público da API.

### Por que separar Controller, Service e Repository?

A separação melhora a organização, facilita testes e torna a aplicação mais simples de evoluir.

Cada camada possui uma responsabilidade clara:

- Controller: entrada HTTP;
- Service: regras da aplicação;
- Repository: acesso ao banco de dados.

### Por que usar paginação?

A paginação evita retornar grandes volumes de dados em uma única resposta.

Isso melhora a performance da API e reduz o custo de tráfego entre cliente e servidor.

### Por que usar cache persistente?

O cache reduz consultas repetidas ao banco em cenários de leitura frequente.

Neste projeto, o cache foi implementado em uma tabela no PostgreSQL/Supabase para permitir:

- visualizar registros cacheados;
- controlar criação e expiração;
- invalidar registros quando os dados originais forem alterados;
- testar o comportamento do cache de forma clara.

### Por que usar trigger para invalidação?

Como o cache fica salvo em banco, a invalidação precisa acompanhar alterações feitas na tabela de origem.

A trigger garante que, quando a tabela `condominios` for alterada, os caches relacionados sejam invalidados mesmo que a alteração não tenha sido feita diretamente pela API.

---

## Próximos passos

- Mapear as tabelas reais que alimentarão o endpoint de Condomínios.
- Validar se o contrato atual da API será mantido.
- Confirmar origem real dos dados de síndico.
- Confirmar se número de torres e unidades virá de coluna ou cálculo.
- Avaliar impacto de múltiplas tabelas na estratégia de cache.
- Evoluir futuramente para os endpoints de Torres e Unidades.

---

## Participantes do desafio

- **Desenvolvedor:** Natan Müller
- **Mentor:** Marcel Guinther
- **Gestor apoiador:** Alexandre Cambraia
- **Tech Lead:** Rodrigo Silva
- **Área:** Tecnologia da Informação