# AuxiAPI

API REST em **.NET 10** para consulta de dados do domínio condominial, desenvolvida no desafio **StartAPI - Da Lógica à Prática na Criação de API**.

O projeto tem como objetivo praticar a construção de uma API REST aplicando conceitos de HTTP, JSON, autenticação, separação de responsabilidades, acesso a banco de dados, tratamento de erros, cache persistente, documentação e testes automatizados.

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
| Token automático em ambiente Development | Implementado |
| Injeção automática de Authorization em Development | Implementada |
| Endpoint `/dev/token` para apoio/debug | Implementado |
| Tratamento global de erros | Implementado |
| Cache persistente em PostgreSQL/Supabase | Implementado |
| Invalidação automática de cache por trigger | Implementada |
| Swagger/OpenAPI | Implementado |
| Página HTML de consulta | Implementada |
| Testes automatizados | 70 testes passando, sem falhas e sem avisos |
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

```http
GET /api/condominios
GET /api/condominios/{id}
```

A API permite:

- listar condomínios com paginação;
- consultar um condomínio por ID;
- filtrar por código do condomínio;
- filtrar por CNPJ com ou sem máscara;
- filtrar por nome ignorando diferenças de caixa e acentuação;
- retornar erros padronizados;
- proteger os endpoints com JWT;
- automatizar o token em ambiente de desenvolvimento;
- cachear consultas por ID e por nome;
- invalidar cache automaticamente quando a tabela `condominios` é alterada;
- consultar manualmente os dados por uma página HTML de apoio.

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
- **HTML, CSS e JavaScript** para a página simples de consulta

---

## Arquitetura e estrutura do projeto

O AuxiAPI utiliza uma arquitetura baseada em camadas, seguindo princípios de separação de responsabilidades para garantir organização, testabilidade e manutenção.

```text
AuxiAPI/
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
│   │   └── DevTokenInjectionMiddlewareTest.cs
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
| Middleware | Padroniza tratamento de erros e, em Development, injeta token automaticamente em chamadas `/api` |
| Security | Centraliza autenticação JWT e automação de token em ambiente de desenvolvimento |
| CacheRepository | Lê e grava registros na tabela de cache |
| TestInfrastructure | Fornece autenticação fake e banco PostgreSQL em container para testes |

### Controller

A camada de controller expõe os endpoints HTTP da API.

No endpoint de Condomínios, o controller disponibiliza:

```http
GET /api/condominios
GET /api/condominios/{id}
```

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

A API não retorna diretamente a entidade do banco. Isso evita acoplamento entre a estrutura interna da aplicação e o contrato público exposto para o cliente.

---

## Endpoint de Condomínios

### Listar condomínios

```http
GET /api/condominios
```

Retorna uma lista paginada de condomínios.

Exemplo:

```http
GET /api/condominios?Pagina=1
```

### Buscar condomínio por ID

```http
GET /api/condominios/{id}
```

Exemplo:

```http
GET /api/condominios/1
```

### Filtros disponíveis

| Filtro | Query param | Observação |
|---|---|---|
| Código | `CodigoDoCondominio` | Aceita `1` ou `0001` |
| CNPJ | `CNPJDoCondominio` | Aceita com ou sem máscara |
| Nome | `NomeDoCondominio` | Ignora caixa e acentuação |
| Página | `Pagina` | Página mínima: 1 |

Exemplos:

```http
GET /api/condominios?CodigoDoCondominio=1
GET /api/condominios?CodigoDoCondominio=0001
GET /api/condominios?CNPJDoCondominio=12345678000101
GET /api/condominios?CNPJDoCondominio=12.345.678/0001-01
GET /api/condominios?NomeDoCondominio=Residencial
GET /api/condominios?NomeDoCondominio=Residencial&Pagina=2
```

O tamanho da página é fixo em **10 itens**.

---

## Exemplos de resposta

### Resposta paginada

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

### Resposta por ID

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

## Autenticação

Os endpoints de Condomínios são protegidos por autenticação JWT Bearer.

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
| `404 Not Found` | Condomínio ou rota não encontrada |
| `500 Internal Server Error` | Erro inesperado |

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

```sql
chave_cache = chave da consulta
AND expirado_em > now()
AND invalidado_em IS NULL
```

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

- escolher tipo de busca;
- listar condomínios;
- buscar por ID, código, CNPJ ou nome;
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

### 6. Compilar e executar

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
| `DTOsTest` | Normalizações e regras dos DTOs |
| `MiddlewaresTest` | Tratamento global de exceções e injeção automática de token em Development |
| `ServicesTest` | Regras de service, cache, paginação e validações |
| `RepositoriesTest` | Consultas, filtros, paginação e cache repository |
| `IntegrationTest` | Endpoints, autenticação, erros e trigger de cache |
| `TestInfrastructure` | Base para autenticação fake e PostgreSQL em container |

Para os testes de integração, mantenha o Docker em execução.

### Cobertura atual validada

A suíte cobre os principais comportamentos da API:

- filtros por código, CNPJ e nome;
- paginação;
- busca por ID;
- autenticação;
- tratamento global de erros;
- cache persistente;
- invalidação automática por trigger;
- repository de cache;
- service de cache;
- endpoint de Condomínios;
- middleware de injeção automática de token em Development.

Classificação funcional da suíte:

| Tipo de teste | Quantidade | O que cobre |
|---|---:|---|
| Testes unitários e de componentes isolados | 38 | Services, DTOs, Controllers e Middlewares |
| Testes de persistência/repository | 19 | Consultas, filtros, paginação e cache repository com PostgreSQL em container |
| Testes de integração | 13 | Endpoints, autenticação, erros e trigger de cache |
| **Total** | **70** | Suíte completa validada |

Resultado da validação atual:

```text
70 testes executados
70 testes passaram
0 falhas
0 ignorados
Build sem avisos
```

---

## Boas práticas aplicadas

- separação entre Controller, Service e Repository;
- uso de DTOs para contrato de API;
- tratamento global de exceções;
- validação de entrada;
- autenticação JWT com validação real do token;
- automação de token restrita ao ambiente Development;
- credenciais sensíveis configuradas via User Secrets;
- middleware de desenvolvimento sem sobrescrever `Authorization` manual;
- endpoint `/dev/token` limitado ao ambiente Development;
- documentação com Swagger/OpenAPI;
- consultas de leitura com `AsNoTracking()`;
- paginação;
- cache persistente em banco;
- invalidação automática de cache;
- migrations sem seed de dados de negócio;
- testes unitários, de persistência e de integração.

---

## Decisões técnicas

| Decisão | Justificativa |
|---|---|
| Uso de DTOs | DTOs evitam expor diretamente as entidades do banco e mantêm o contrato da API mais estável. |
| Separação entre Controller, Service e Repository | A divisão por camadas organiza responsabilidades e facilita manutenção e testes. |
| Paginação fixa | A paginação evita retornos muito grandes e mantém o comportamento da listagem previsível. |
| Cache persistente | O cache em banco permite reduzir consultas repetidas e facilita visualizar, expirar e invalidar respostas armazenadas. |
| Invalidação por trigger | A trigger garante que alterações na tabela `condominios` invalidem caches relacionados mesmo fora do fluxo da API. |
| Migrations sem carga de dados | As migrations versionam a estrutura do banco, enquanto dados de negócio ficam separados da evolução do schema. |
| Autenticação JWT | O JWT protege os endpoints e mantém a validação real de acesso via pipeline da aplicação. |
| Token automático em Development | A automação reduz retrabalho em testes locais sem remover `[Authorize]` nem desativar a validação JWT. |
| User Secrets | Credenciais sensíveis ficam fora do repositório e não são expostas no código-fonte. |
| Testes automatizados | A suíte valida regras, filtros, cache, autenticação, endpoints, erros e persistência antes da entrega. |

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