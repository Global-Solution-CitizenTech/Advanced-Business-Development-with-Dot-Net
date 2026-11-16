# 🤖 AGENT.md — Diretrizes Arquiteturais, Boas Práticas & Engenharia de Software (.NET 10 / C# 14)

> **Projeto:** **ZelaFix API** — Plataforma de Zeladoria Urbana Colaborativa (CitizenTech / ODS 11).  
> **Instituição:** FIAP (2TDSPG — Global Solution 2026/2 — *Advanced Business Development with .NET*).  
> **Escopo:** Documento normativo, técnico e operacional contendo decisões arquiteturais, padrões de design, convenções de código, Clean Architecture, DDD, SOLID, KISS, YAGNI, DRY, Clean Code e a suíte de testes AAA utilizados no ecossistema **ZelaFix**.

---

## 🏛️ 1. Visão Geral da Arquitetura (Clean Architecture & DDD)

A arquitetura organiza a aplicação em 4 camadas desacopladas com fluxo unidirecional e estrita Inversão de Dependência (DIP):

```text
┌────────────────────────────────────────────────────────────────────────┐
│                              ZelaFix.Api                               │ ──> Minimal APIs RESTful, JWT Auth, Idempotência,
└───────────────────────────────────┬────────────────────────────────────┘     Rate Limiting, Health Checks, Swagger UI
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│                          ZelaFix.Application                           │ ──> Casos de Uso, DTOs (Records), Validações,
└───────────────────────┬────────────────────────┬───────────────────────┘     Contratos de Repositório e Serviços
                        │                        │
┌───────────────────────▼───────┐        ┌───────▼───────────────────────┐
│        ZelaFix.Domain         │        │     ZelaFix.Infrastructure    │
│ (POCOs, Invariantes, Enums)   │        │ (EF Core Oracle + Repositórios│
└───────────────────────────────┘        └───────────────────────────────┘
```

### Regras de Dependência (DIP) & Governança Arquitetural:
1. **`ZelaFix.Domain` (Núcleo Puro & Agnóstico):**
   * Contém entidades de negócio (`Chamado`, `Categoria`, `Usuario`, `Endereco`, `Bairro`, `Cidade`, `Estado`, `Telefone`, `Status`, `Prioridade`, `OrgaoGoverno`, `HistChamado`, `VotoChamado`), enums (`Cargo`), exceções semânticas (`DomainException`) e regras de domínio puras.
   * Totalmente agnóstico de frameworks, persistência ou bibliotecas de terceiros (zero EF Core ou ASP.NET Core no domínio).
2. **`ZelaFix.Application` (Contratos, DTOs & Orquestração):**
   * DTOs modelados com C# 14 `record` imutáveis, validações declarativas com `System.ComponentModel.DataAnnotations` e sanitização.
   * Contratos de persistência (`IChamadoRepository`, `IUsuarioRepository`, `ICategoriaRepository`, `IUnitOfWork`) e interfaces de serviços de domínio (`IChamadoService`, `IAuthService`, `ICategoriaService`, `ITokenService`, `IEnderecoResolver`, `IViaCepService`).
   * **Zero Dependência NoSQL ou LLM:** Persistência estritamente relacional em Oracle Database, sem dependências artificiais de MongoDB ou APIs externas de inteligência generativa.
3. **`ZelaFix.Infrastructure` (Persistência Relacional EF Core):**
   * Contexto relacional `ZelaFixDbContext` (Oracle via `Oracle.EntityFrameworkCore`).
   * Configurações Fluent API desacopladas (`IEntityTypeConfiguration<T>`) mapeando nomes de tabelas físicos em maiúsculo (`CHAMADO`, `CATEGORIA`, etc.), constraints de validação de banco (`CHECK`) e tipos específicos (`BLOB`, `DATE`, `NUMBER(5)`).
   * Repositórios genéricos e especializados (`ChamadoRepository`, `UsuarioRepository`, `UnitOfWork`) operando via Entity Framework Core puro.
4. **`ZelaFix.Api` (Hosting, Minimal APIs & Borda HTTP):**
   * ASP.NET Core Minimal APIs de alta performance com mapeamento modularizado (`MapAuthEndpoints`, `MapCategoriaEndpoints`, `MapChamadoEndpoints`).
   * Segurança JWT Bearer com roles baseadas em claims (`ClaimsNomes.Perfil`: `CIDADAO` / `GESTOR`), middleware de idempotência com chave `Idempotency-Key`, rate limiting nativo Fixed Window e tratamento global de exceções (RFC 7807 Problem Details).

---

## 🎯 2. Padrões por Camada (Implementação Prática)

### 2.1 Camada de Domínio (`ZelaFix.Domain`)

* **Identidade & BaseEntity:**
  * Entidades herdam de `BaseEntity` com identificadores inteiros mapeados como `NUMBER(5)` no Oracle (`Id`).
* **Segurança Criptográfica de Senhas (`HashHelper` com BCrypt):**
  * Senhas nunca são salvas em texto puro. O `HashHelper` implementa hash criptográfico seguro com salt embutido via `BCrypt.Net-Next`.
  * Validações de tamanho mínimo (8 caracteres), letra maiúscula, caractere especial e número são executadas na borda e no domínio.
* **Invariantes & Encapsulamento:**
  * Estados válidos protegidos por construtores semânticos e métodos de mutação (`Atualizar(...)`, `AlterarStatus(...)`, `RegistrarVoto(...)`).
  * Tentativas de transição de estado inválidas (ex.: mover chamado concluído para pendente sem permissão, ou cidadão alterar chamado em execução) lançam `DomainException`.
* **Políticas de Prioridade Comunitária:**
  * `PoliticaPrioridade`: cálculo dinâmico da criticidade e escalonamento de prioridade baseado no acúmulo de apoios comunitários de munícipes da vizinhança.

---

### 2.2 Camada de Aplicação (`ZelaFix.Application`)

* **DTOs Imutáveis com Records:**
  * `CriarChamadoRequest`, `AtualizarChamadoRequest`, `AlterarStatusRequest`, `CategoriaRequest`, `RegisterRequest`, `LoginRequest`.
  * Validações de schema via DataAnnotations (`[Required]`, `[StringLength]`, `[RegularExpression(@"^\d{5}-?\d{3}$")]` para CEP, `[Range(1, 4)]` para prioridades).
* **Transferência de Evidências Fotográficas (Base64 / BLOB):**
  * Chamados suportam imagens em formato Base64 (JPEG, PNG, WebP até 5 MB) no DTO, convertidas em bytes de array (`byte[]`) e persistidas como `BLOB` na coluna `FOTO` do Oracle, garantindo integridade e portabilidade sem necessidade de bucket S3 externo.
* **Resolução Inteligente de Endereços (`IEnderecoResolver` & `IViaCepService`):**
  * Cidadão informa apenas CEP e número; o serviço consulta o ViaCEP de forma resiliente (HTTP timeout 5s) e normaliza/reutiliza `Bairro`, `Cidade` e `Estado` no banco relacional.
* **Serviços Coesos (KISS / YAGNI):**
  * `AuthService`, `CategoriaService`, `ChamadoService`, `TokenService`, `EnderecoResolver`.
  * Métodos curtos, diretos e sem abstrações desnecessárias (Ponytail dev style).

---

### 2.3 Camada de Infraestrutura (`ZelaFix.Infrastructure`)

* **Mapeamento Fluent API Isolado:**
  * Cada tabela possui configuração isolada em `Persistence/Configurations/`:
    - `CadastroConfigurations.cs` (`CategoriaConfiguration`, `StatusConfiguration`, `PrioridadeConfiguration`, `OrgaoGovernoConfiguration`)
    - `ChamadoConfigurations.cs` (`ChamadoConfiguration`, `HistChamadoConfiguration`, `VotoChamadoConfiguration`)
    - `LocalizacaoConfigurations.cs` (`EstadoConfiguration`, `CidadeConfiguration`, `BairroConfiguration`, `EnderecoConfiguration`)
    - `UsuarioConfigurations.cs` (`UsuarioConfiguration`, `TelefoneConfiguration`)
* **Transações Atômicas e Repositórios (`IUnitOfWork`):**
  * Todas as operações de mutação coordenada (abertura de chamado com histórico inicial, atualização de status e votos populares) são orquestradas via `IUnitOfWork` garantindo transações ACID puras no EF Core.

---

### 2.4 Camada de Apresentação & Hosting (`ZelaFix.Api`)

* **Minimal APIs Enxutas:**
  * Roteamento limpo agrupado em `/api/v1/auth`, `/api/v1/categorias` e `/api/v1/chamados`.
* **Idempotência de Requisições Críticas (`WithIdempotency`):**
  * Operações sensíveis a duplicidade (`POST /api/v1/chamados` e `PATCH /api/v1/chamados/{id}/status`) aceitam o cabeçalho HTTP `Idempotency-Key`.
  * Requisições repetidas com a mesma chave retornam o resultado original cacheado com o cabeçalho `Idempotent-Replayed: true`.
* **Rate Limiting Nativo (Fixed Window):**
  * Proteção contra DDoS e força bruta com políticas do ASP.NET Core:
    - `GlobalPolicy`: 100 requisições / minuto por IP.
    - `AuthPolicy`: 10 tentativas / minuto para login e registro.
* **Tratamento Global de Exceções (`GlobalExceptionHandler` / RFC 7807):**
  * Captura `DomainException` (400), `KeyNotFoundException` (404), `InvalidOperationException` (409) e erros não tratados (500), devolvendo JSON padronizado `ProblemDetails` com `TraceId` e timestamp.
* **Observabilidade & Health Checks:**
  * Endpoints `/health`, `/health/ready` e `/health/live` reportando status em JSON estruturado com métricas de tempo de resposta do banco Oracle e ViaCEP.

---

## 🧪 3. Padrão de Testes Automatizados (AAA & xUnit/Moq)

### 3.1 Convenção de Nomenclatura
```text
MetodoTestado_Cenario_ResultadoEsperado
```
*Exemplos:*
- `CriarAsync_DadosValidos_DeveRetornarChamadoCriado`
- `AlterarStatusAsync_ChamadoNaoEncontrado_DeveLancarExcecao`
- `ApoiarAsync_AutorApoiandoProprioChamado_DeveLancarDomainException`
- `LoginAsync_CredenciaisInvalidas_DeveRetornarNull`

### 3.2 Estrutura da Suíte de Testes
* **Testes Unitários (`ZelaFix.Tests.Unit` — 42 testes):**
  * Domínio (`EnderecoTests`, `StatusTests`, `UsuarioTests`).
  * Aplicação (`AuthServiceTests`, `CategoriaServiceTests`, `ChamadoServiceTests`, `FotoImagemTests`, `TokenServiceTests`).
* **Testes de Integração (`ZelaFix.Tests.Integration` — 35 testes):**
  * `CustomWebApplicationFactory` com banco de dados em memória e fakes (`FakeViaCepService`).
  * Cobertura de autenticação, permissões RBAC (`GestorOnly`), rate limiting, idempotência, ciclo completo de chamados, HATEOAS, Correlation ID e health checks.
* **Total:** 77 testes automatizados executando com 100% de aprovação (zero falhas e zero warnings).

---

## ✅ 4. Checklist de Conformidade da Solução

- [x] **Domínio:** Entidades POCOs com validações de invariantes, sem acoplamento a frameworks externos?
- [x] **Segurança:** Senhas protegidas com `HashHelper` e BCrypt com salt nativo?
- [x] **Zero NoSQL / Zero LLM:** Solução fundamentada estritamente no Oracle Database relacional, sem MongoDB e sem LLM?
- [x] **Minimal APIs:** Endpoints estruturados sob `/api/v1/` com TypedResults e OpenAPI/Swagger na raiz (`/`)?
- [x] **Idempotência:** Middleware ativo com header `Idempotency-Key` e retorno `Idempotent-Replayed: true`?
- [x] **Rate Limiting:** Políticas nativas ativas contra abuso de endpoints públicos e autenticação?
- [x] **Autenticação & RBAC:** JWT Bearer com claims `CIDADAO` e `GESTOR` e política granular `GestorOnly`?
- [x] **Persistência Oracle:** Mapeamento completo via Fluent API das 14 tabelas relacionais em EF Core puro (sem PL/SQL)?
- [x] **Observabilidade:** RFC 7807 Problem Details global, Serilog com `X-Correlation-ID` e Health Checks estruturados (`/health`, `/health/ready`, `/health/live`)?
- [x] **Testes Automatizados:** Suíte com 100% de sucesso em `dotnet test` (77 testes passando: 42 unitários + 35 integração)?
