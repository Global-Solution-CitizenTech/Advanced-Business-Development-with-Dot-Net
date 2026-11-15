# ZelaFix API — Zeladoria Urbana Colaborativa

> **FIAP Global Solution 2026/2 — Advanced Business Development with .NET (2TDSPG)**
> Tema CitizenTech · ODS 11 — Cidades e Comunidades Sustentáveis

## Visão Geral
API REST (.NET 10, Minimal API) que sustenta a plataforma **ZelaFix**: cidadãos abrem chamados de zeladoria urbana (buracos, iluminação pública, descarte irregular, vazamentos, galerias pluviais), anexam uma foto, apoiam ocorrências da vizinhança e acompanham o ciclo de vida do atendimento público até sua resolução. Gestores públicos analisam, tramitam os status e fiscalizam os prazos de atendimento.

**Benefício para o Cidadão & Gestão Pública:**
- Abertura rápida com geolocalização e envio de imagem fotográfica em BLOB.
- Atribuição inteligente de órgãos municipais competentes de acordo com a tipologia da ocorrência.
- Mobilização popular por meio do engajamento comunitário (votos/apoios de munícipes), promovendo escalonamento dinâmico de prioridade (padrão de pressão comunitária).
- Transparência total através da trilha histórica de auditoria de cada chamado.

## Arquitetura e Boas Práticas
Desenvolvida seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**:
- `ZelaFix.Domain`: Entidades ricas com validações de invariantes de negócio, regras de domínio e exceções semânticas.
- `ZelaFix.Application`: Casos de uso, DTOs (Request/Response Models), contratos de repositórios, serviços de domínio e mapeamento.
- `ZelaFix.Infrastructure`: Persistência relacional em banco de dados **Oracle** via **Entity Framework Core**, repositórios genéricos e especializados, mapeamento de procedures PL/SQL e Unit of Work.
- `ZelaFix.Api`: ASP.NET Core Minimal APIs, autenticação e autorização via JWT, rate limiting nativo, middleware de idempotência, tratamento global de erros (RFC 7807 Problem Details), Health Checks e documentação interativa Swagger/OpenAPI.

## Requisitos Técnicos Atendidos (Rubrica GS2 - 100 pts)

| Requisito | Implementação | Pontuação |
|---|---|:---:|
| **Minimal API RESTful & Swagger** | Endpoints semânticos, validações de payload com DataAnnotations, status codes padronizados (200, 201, 204, 400, 401, 403, 404, 409, 422, 429, 500) e OpenAPI documentado em `/`. | **20 pts** |
| **Autenticação & Autorização JWT** | Proteção de endpoints via Tokens JWT com assinatura HMAC-SHA256, claims de perfil (`CIDADAO` / `GESTOR`) e política de autorização granular (`GestorOnly`). | **20 pts** |
| **Idempotência de Requisições** | Middleware de idempotência com cabeçalho `Idempotency-Key` em operações críticas (`POST /chamados` e `PATCH /chamados/{id}/status`), prevenindo registros duplicados e retornando `Idempotent-Replayed: true` em repetições. | **15 pts** |
| **Rate Limiting Nativo** | Políticas nativas de rate limiting com Fixed Window (`GlobalPolicy` e `AuthPolicy`), retornando `429 Too Many Requests` com cabeçalho `Retry-After`. | **15 pts** |
| **Testes Automatizados (xUnit)** | Bateria com mais de 70 testes automatizados cobrindo regras de negócio unitárias (Moq) e testes de integração de ponta a ponta (`WebApplicationFactory` com banco InMemory). | **20 pts** |
| **Health Checks & Observabilidade** | Endpoints `/health`, `/health/ready` e `/health/live` monitorando integridade da API, banco de dados Oracle e serviços de terceiros (ViaCEP). | **10 pts** |

## Catálogo de Endpoints (`/api/v1`)

### Autenticação (`/api/v1/auth`)
| Método | Endpoint | Perfil | Descrição |
|---|---|---|---|
| POST | `/api/v1/auth/register` | Público | Cadastra um cidadão (endereço resolvido via CEP) e emite token JWT. |
| POST | `/api/v1/auth/login` | Público | Autentica usuário com e-mail e senha, retornando token JWT. |
| GET | `/api/v1/auth/me` | Autenticado | Retorna perfil e dados cadastrais do usuário autenticado. |

### Categorias de Zeladoria (`/api/v1/categorias`)
| Método | Endpoint | Perfil | Descrição |
|---|---|---|---|
| GET | `/api/v1/categorias` | Autenticado | Lista o catálogo oficial de categorias de zeladoria. |
| GET | `/api/v1/categorias/{id}` | Autenticado | Obtém detalhes de uma categoria específica. |
| POST | `/api/v1/categorias` | Gestor | Cadastra nova categoria de serviço público. |
| PUT | `/api/v1/categorias/{id}` | Gestor | Atualiza nomenclatura de uma categoria. |
| DELETE | `/api/v1/categorias/{id}` | Gestor | Remove uma categoria (bloqueia com 409 se possuir chamados). |

### Chamados Urbanos (`/api/v1/chamados`)
| Método | Endpoint | Perfil | Descrição |
|---|---|---|---|
| GET | `/api/v1/chamados` | Autenticado | Consulta paginada com filtros (`categoriaId`, `bairroId`, `status`, `busca`, `apenasMeus`, `incluirFoto`). |
| GET | `/api/v1/chamados/{id}` | Autenticado | Detalha ocorrência completa com histórico de status e foto em Base64. |
| POST | `/api/v1/chamados` | Autenticado | Abre novo chamado com título, descrição, foto, categoria e endereço (exige `Idempotency-Key`). |
| PUT | `/api/v1/chamados/{id}` | Autor / Gestor | Edita dados da ocorrência (autor apenas enquanto status `PENDENTE`). |
| PATCH | `/api/v1/chamados/{id}/status` | Gestor | Movimenta o chamado no fluxo operacional (exige `Idempotency-Key`). |
| POST | `/api/v1/chamados/{id}/apoiar` | Autenticado | Registra voto/apoio de vizinho (autor não pode apoiar o próprio chamado; apoios escalam prioridade). |
| DELETE | `/api/v1/chamados/{id}/apoiar` | Autenticado | Remove apoio comunitário previamente concedido. |
| DELETE | `/api/v1/chamados/{id}` | Autor / Gestor | Exclui chamado (autor apenas enquanto `PENDENTE`). |

### Gestão Territorial & Engajamento (`/api/v1/bairros/{id}`)
| Método | Endpoint | Perfil | Descrição |
|---|---|---|---|
| GET | `/api/v1/bairros/{id}/criticidade` | Autenticado | Retorna a classificação de criticidade urbana calculada no Oracle. |
| GET | `/api/v1/bairros/{id}/engajamento` | Autenticado | Retorna relatório territorial consolidado em JSON nativo do banco. |

## Ciclo de Vida do Chamado (Status FSM)
- `PENDENTE`: Ocorrência registrada pelo cidadão, aguardando triagem do fiscal.
- `EM_ANALISE`: Em vistoria técnica pela equipe da subprefeitura ou concessionária.
- `EM_EXECUCAO`: Ordem de serviço emitida e equipe em campo realizando a manutenção.
- `CONCLUIDO`: Reparo concluído e fiscalizado.
- `REJEITADO`: Solicitação indeferida (duplicidade ou fora de competência municipal).

## Como Executar Localmente

### 1. Pré-requisitos
- .NET 10 SDK instalado
- Docker Desktop rodando

### 2. Inicializar o Banco de Dados Oracle via Docker
Você pode utilizar o container local Oracle executando:

```bash
docker run -d --name oracle-zelafix -p 1521:1521 -e ORACLE_PASSWORD=oracle -e APP_USER=zelafix -e APP_USER_PASSWORD=zelafix gvenzl/oracle-free:slim
```

Aguarde a inicialização do container e aplique os scripts SQL localizados em `scripts/`:
```bash
docker cp scripts/2TDSPG_2026_CodigoSql_ZelaFix.sql oracle-zelafix:/tmp/ddl.sql
docker cp scripts/2TDSPG_2026_Pacotes_ZelaFix.sql oracle-zelafix:/tmp/packages.sql
docker exec oracle-zelafix bash -c "sqlplus -s zelafix/zelafix@//localhost:1521/FREEPDB1 @/tmp/ddl.sql"
docker exec oracle-zelafix bash -c "sqlplus -s zelafix/zelafix@//localhost:1521/FREEPDB1 @/tmp/packages.sql"
```

### 3. Configurar Conexão e Executar a API
No ambiente de desenvolvimento (`Development`), a API já aponta por padrão para `localhost:1521/FREEPDB1`.
Para rodar a aplicação:

```powershell
cd ZelaFix.Api
dotnet run
```

Acesse a interface interativa Swagger no navegador:
```
http://localhost:5034/
```

### 4. Executar os Testes Automatizados
```powershell
dotnet test
```

## Monitoramento e Health Checks
- `GET /health`: Diagnóstico completo da API, persistência Oracle e ViaCEP.
- `GET /health/ready`: Readiness probe (verifica integridade das dependências externas).
- `GET /health/live`: Liveness probe (indica que o processo da aplicação está ativo).

## Integrantes
| Nome | RM | Turma | GitHub | LinkedIn |
| :--- | :---: | :---: | :--- | :--- |
| **Enzo Okuizumi** | **561432** | 2TDSPG | [EnzoOkuizumiFiap](https://github.com/EnzoOkuizumiFiap) | [Enzo Okuizumi](https://www.linkedin.com/in/enzo-okuizumi-b60292256/) |
| **Gustavo Okada** | **563428** | 2TDSPG | [Gdev3356](https://github.com/Gdev3356) | [Gustavo Okada](https://www.linkedin.com/in/gustavo-okada-53a3b8359/) |
| **Lucas Barros Gouveia** | **566422** | 2TDSPG | [LuzBGouveia](https://github.com/LuzBGouveia) | [Lucas Barros Gouveia](https://www.linkedin.com/in/lucas-barros-gouveia-09b147355/) |
| **Luna de Carvalho Guimarães** | **562290** | 2TDSPG | [lunaguima](https://github.com/lunaguima) | [Luna Guimarães](https://www.linkedin.com/in/luna-guimar%C3%A3es-b0ba82309/) |
| **Milton Jakson de Sousa Marcelino** | **564836** | 2TDSPG | [MiltonMarcelino](https://github.com/MiltonMarcelino) | [Milton Marcelino](https://www.linkedin.com/in/milton-marcelino-250298142) |
