# AuxiAPI

API REST em .NET 10 desenvolvida para o desafio StartAPI.

O escopo atual implementado é o endpoint de Condomínios, com:

- listagem paginada;
- busca por ID;
- filtros por query params;
- autenticação JWT;
- cache em memória;
- tratamento global de erros;
- testes automatizados;
- documentação com Fumadocs.

## Documentação

A documentação completa está em:

`content/docs/api/condominios.mdx`

## Como rodar

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AuxiAPI.WebApi.csproj