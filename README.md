# AuxiAPI - Desafio StartAPI (Auxiliadora Predial)

Este repositório contém a **AuxiAPI**, uma API REST desenvolvida em **.NET 8 (C#)** voltada para a gestão e consulta estrutural de dados do domínio condominial da **Auxiliadora Predial**. 

O projeto faz parte do desafio **"StartAPI - Da Lógica à Prática na Criação de API"**, cujo propósito central é consolidar conhecimentos práticos de desenvolvimento back-end, arquitetura em camadas, resiliência defensiva e segurança da informação.

---

## 🚀 O Desafio & Contexto de Negócio

A missão prática consiste em mapear e expor o fluxo estrutural de dados do domínio habitacional em três entidades fundamentais:
1. **Condomínios**: Listagem e consulta das informações gerais.
2. **Torres**: Blocos vinculados aos respectivos condomínios.
3. **Unidades**: Apartamentos, casas ou salas pertencentes a cada torre.

O foco atual desta entrega é a implementação e consolidação do **Endpoint de Condomínios**, assegurando uma separação estrita de responsabilidades, tratamento customizado de erros (evitando o vazamento de exceções brutas para o cliente) e proteção de dados.

---

## 🛠️ Arquitetura do Projeto (`src`)

A API adota o **Repository Pattern** em conjunto com os princípios de **Separação de Responsabilidades (SoC)**, resultando em um código desacoplado, manutenível e altamente testável.

Abaixo está a organização e o mapeamento detalhado da árvore de diretórios do projeto dentro da pasta `src`:

```text
AUXIAPI/
└── src/
    ├── Common/               # Componentes transversais reutilizáveis
    │   ├── Cache/            # Abstrações e implementações de Caching (IMemoryCache)
    │   └── MensagensDeErro.cs# Centralização de strings de validação e mensagens de erro
    ├── Contexts/             # Camada de Dados e Persistência (ORM)
    │   └── CondominiosDbContext.cs # Contexto do EF Core e Seed de dados estruturados
    ├── Controllers/          # Camada de Exposição (Porta de entrada HTTP)
    │   └── CondominiosController.cs # Endpoints (GET) para listagem geral e busca por ID
    ├── DTOs/                 # Data Transfer Objects (Contratos de entrada e saída)
    │   ├── InformacoesCondominioDto.cs # Objeto de resposta limpo retornado ao cliente
    │   └── VisualizarCondominioQuery.cs# Parâmetros de filtro aceitos na URL (Query Params)
    ├── Entities/             # Domínio Puro (Modelos relacionais do banco)
    │   └── Condominio.cs     # Mapeamento de atributos da tabela 'condominios'
    ├── Middlewares/          # Interceptadores globais de requisições
    │   ├── GlobalExceptionHandler.cs # Captura automática de falhas e padronização JSON
    │   └── ValidarModelStateFilter.cs # Filtro para validação automática de contratos
    ├── Migrations/           # Histórico de evolução do esquema de banco de dados
    ├── Properties/           # Configurações de inicialização do ambiente local
    ├── Repositories/         # Abstração de acesso e persistência a dados
    │   ├── ICondominioRepository.cs  # Interface de desacoplamento do repositório
    │   └── CondominioRepository.cs   # Implementação LINQ/EF Core com As NoTracking()
    ├── Services/             # Camada de Negócio (Orquestração, Cache e Regras)
    │   └── CondominioService.cs # Validações de tamanho, normalização e controle de cache
    ├── appsettings.json      # Configurações gerais de ambiente (sem credenciais expostas)
    ├── appsettings.Development.json
    ├── AuxiAPI.WebApi.csproj # Definição do projeto e dependências de pacotes NuGet
    ├── AuxiAPI.WebApi.http   # Arquivo para testes rápidos internos de chamadas HTTP
    └── Program.cs            # Injeção de dependências, middlewares e pipeline da aplicação
```

### Divisão de Responsabilidades:
* **Controllers**: Recebe as requisições HTTP, repassa os dados de entrada para a camada de serviço e responde com os status codes adequados (`200 OK`, `401 Unauthorized`, `404 Not Found`).
* **Services**: Orquestra as regras de negócio. Responsável por validar comprimentos de strings, normalizar buscas e decidir se os dados devem vir do **In-Memory Cache** ou se há necessidade de acessar o banco.
* **Repositories**: Comunica-se de forma direta com o PostgreSQL hospedado no Supabase. Utiliza otimizações como `.AsNoTracking()` para leituras rápidas e resoluções de filtros dinâmicos de texto via `ILike`.

---

## 🔒 Segurança e Gestão de Credenciais

### 1. User Secrets (Ambiente de Desenvolvimento)
Visando eliminar o risco de vazamento de credenciais em repositórios públicos, as strings de conexão com o banco de dados contendo senhas reais foram **completamente removidas** do arquivo `appsettings.json`. Em ambiente local, a aplicação utiliza a ferramenta nativa **User Secrets** do .NET, armazenando os dados confidenciais fora do diretório rastreado pelo Git.

### 2. Autenticação JWT Nativa do Supabase
A API está integrada ao middleware de autenticação da Microsoft para realizar a validação assimétrica dos tokens **JWT** emitidos pelo **Supabase Auth**.
* O controller `/api/Condominios` é protegido pelo atributo `[Authorize]`, exigindo validação prévia.
* Requisições não autenticadas ou com tokens expirados são bloqueadas na entrada com o status **`401 Unauthorized`**.
* O Swagger UI foi configurado para suportar o fluxo `Bearer Token`, inserindo o cabeçalho `Authorization` nativamente para testes na interface gráfica.

---

## ⚙️ Configuração e Execução Local

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* Acesso ou credenciais de um banco de dados PostgreSQL/Supabase ativo.

### Passo a Passo

1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/natemuller/AuxiAPI.git](https://github.com/natemuller/AuxiAPI.git)
   cd AuxiAPI
   ```

2. **Configure as Credenciais de Desenvolvimento (User Secrets):**
   Execute o seguinte comando no terminal (dentro do diretório raiz) para cadastrar a String de Conexão com o banco na sua máquina local:
   ```bash
   dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "Host=aws-1-sa-east-1.pooler.supabase.com;Database=postgres;Username=postgres.gsmzasmtlllvzpjppfom;Password=SUA_SENHA_AQUI;SSL Mode=Require;Trust Server Certificate=true"
   ```

3. **Restaure as dependências e faça o build:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Inicie a aplicação:**
   ```bash
   dotnet run --project src/AuxiAPI.WebApi.csproj
   ```

5. **Acesse o Swagger UI:**
   O terminal exibirá as portas HTTPS locais geradas (ex: `https://localhost:7277`). Navegue até `https://localhost:XXXX/swagger` para interagir visualmente com a documentação da API.

---

## 👥 Equipe e Envolvidos no Desafio

* **Desenvolvedor:** Natan (Estagiário Back-End)
* **Mentor:** Marcel Guinther
* **Gestor Apoiador:** Alexandre Cambraia
* **Tech Lead:** Rodrigo Silva
* **Área:** Tecnologia da Informação — **Auxiliadora Predial**