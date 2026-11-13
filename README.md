# ZelaFix API — Zeladoria Urbana Colaborativa

> **FIAP Global Solution 2026/2 — Advanced Business Development with .NET (2TDSPG)**
> Tema CitizenTech · ODS 11 — Cidades e Comunidades Sustentáveis

## Visão geral
API REST (.NET 10, Minimal API) que sustenta o app **ZelaFix Mobile**: cidadãos abrem chamados de problemas urbanos
(buracos, iluminação, descarte irregular...), anexam uma foto, apoiam as ocorrências da vizinhança e acompanham o
histórico até a resolução pela prefeitura. Gestores atualizam o status.

**Benefício para o negócio:** prioriza o que a comunidade considera mais urgente (apoios), dá rastreabilidade ao
atendimento (histórico auditável) e reduz o retrabalho da prefeitura.

## Arquitetura
Clean Architecture: `Domain` → `Application` → `Infrastructure` → `Api`. Persistência **Oracle** via EF Core.
Regras de negócio críticas vivem no banco (pacotes PL/SQL `pkg_chamado_gestao` e `pkg_engajamento_cidadao`) e são
chamadas pela API: abertura de chamado, mudança de status, voto, criticidade e relatório de engajamento.
A **foto é gravada em BLOB** no banco e trafega em Base64 no JSON (sem URL, sem bucket).

## Requisitos atendidos
| Requisito | Onde |
|---|---|
| Minimal API + OpenAPI/Swagger | `Api/Endpoints`, Swagger em `/` |
| JWT + autorização por perfil (`CIDADAO`/`GESTOR`) | `AuthExtensions`, política `GestorOnly` |
| Idempotência (`Idempotency-Key`) | `Api/Idempotency` — `POST /chamados` e `PATCH /chamados/{id}/status` |
| Rate limiting + HTTP 429 (`Retry-After`) | `RateLimitingExtensions` — global e política `auth` |
| Testes xUnit (unit + integração) | `ZelaFix.Tests.Unit`, `ZelaFix.Tests.Integration` |
| Health Checks (API, Oracle, ViaCEP) | `/health`, `/health/ready`, `/health/live` |

## Endpoints (`/api/v1`)
| Método | Rota | Perfil |
|---|---|---|
| POST | `/auth/register`, `/auth/login` | público (rate limit rígido) |
| GET | `/auth/me` | autenticado |
| GET | `/categorias`, `/categorias/{id}` | autenticado |
| POST/PUT/DELETE | `/categorias` | **gestor** |
| GET | `/chamados` (filtros, paginação, `incluirFoto`), `/chamados/{id}` | autenticado |
| POST | `/chamados` (`Idempotency-Key`) | autenticado |
| PUT/DELETE | `/chamados/{id}` | autor (enquanto ABERTO) ou gestor |
| PATCH | `/chamados/{id}/status` (`Idempotency-Key`) | **gestor** |
| POST/DELETE | `/chamados/{id}/apoiar` | autenticado (autor não apoia o próprio) |
| GET | `/bairros/{id}/criticidade`, `/bairros/{id}/engajamento` | autenticado |

Status: `ABERTO`, `EM_ANALISE`, `EM_EXECUCAO`, `CONCLUIDO`, `REJEITADO`.

## Monitoramento
| Endpoint | Verifica |
|---|---|
| `GET /health` | API + Oracle + ViaCEP |
| `GET /health/ready` | Oracle + ViaCEP |
| `GET /health/live` | processo ativo |

Resposta: JSON com `status` e a lista `checks`. ViaCEP indisponível = `Degraded` (a API segue de pé).

## Como executar
Pré-requisitos: .NET 10 SDK e acesso ao Oracle (scripts em `scripts/`: `script_bd.sql` e `pacotes_bd.sql`).

```powershell
dotnet user-secrets init --project ZelaFix.Api
dotnet user-secrets set "ConnectionStrings:ZelaFixOracle" "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=HOST:1521/SERVICO;" --project ZelaFix.Api
dotnet user-secrets set "Jwt:Key" "<chave aleatória com 32+ caracteres>" --project ZelaFix.Api
dotnet run --project ZelaFix.Api
```
Swagger: `http://localhost:5034/`. Em produção as chaves vêm de variáveis de ambiente
(`ConnectionStrings__ZelaFixOracle`, `Jwt__Key`). **Nenhuma credencial é versionada.**

## Testes
```powershell
dotnet test                                                  # todos
dotnet test ZelaFix.Tests.Unit                               # Domain + Application (Moq)
dotnet test ZelaFix.Tests.Integration                        # endpoints (WebApplicationFactory, InMemory)
```
Os testes de integração rodam sem Oracle: o banco vira InMemory e as procedures PL/SQL são substituídas por um fake.

## Integrantes
| Nome | RM | Turma | GitHub | LinkedIn |
| :--- | :---: | :---: | :--- | :--- |
| **Enzo Okuizumi** | **561432** | 2TDSPG | [EnzoOkuizumiFiap](https://github.com/EnzoOkuizumiFiap) | [Enzo Okuizumi](https://www.linkedin.com/in/enzo-okuizumi-b60292256/) |
| **Gustavo Okada** | **563428** | 2TDSPG | [Gdev3356](https://github.com/Gdev3356) | [Gustavo Okada](https://www.linkedin.com/in/gustavo-okada-53a3b8359/) |
| **Lucas Barros Gouveia** | **566422** | 2TDSPG | [LuzBGouveia](https://github.com/LuzBGouveia) | [Lucas Barros Gouveia](https://www.linkedin.com/in/lucas-barros-gouveia-09b147355/) |
| **Luna de Carvalho Guimarães** | **562290** | 2TDSPG | [lunaguima](https://github.com/lunaguima) | [Luna Guimarães](https://www.linkedin.com/in/luna-guimar%C3%A3es-b0ba82309/) |
| **Milton Jakson de Sousa Marcelino** | **564836** | 2TDSPG | [MiltonMarcelino](https://github.com/MiltonMarcelino) | [Milton Marcelino](https://www.linkedin.com/in/milton-marcelino-250298142) |
