# MeuCondomínio — Ecossistema Condominial SaaS Multi-Tenant

> Plataforma de gestão condominial extensível. Inicia como Gestor de Encomendas e cresce para cobrir Avisos, Reservas e muito mais — sem refatorar o núcleo.

---

## Estrutura do Repositório

```
MeuCondominio/
├── backend/
│   └── src/
│       ├── MeuCondominio.Domain/     # Entidades, Enums, Estados, Interfaces
│       ├── MeuCondominio.Application/# UseCases (CQRS/MediatR), OCR Strategy, Builder
│       ├── MeuCondominio.Infrastructure/
│       │   ├── Messaging/            # MetaWhatsAppService
│       │   └── Persistence/
│       │       ├── Migrations/       # Scripts SQL versionados (DbUp)
│       │       │   └── V001__InitialSchema.sql
│       │       ├── DatabaseMigrationRunner.cs
│       │       └── Repositories/     # Implementações Dapper
│       └── MeuCondominio.API/        # Controllers, Program.cs, appsettings.json
│
├── mobile/
│   └── lib/
│       ├── core/                     # ApiClient, SecureStorage, Theme, Router
│       └── features/
│           ├── auth/                 # Login BLoC + SecureStorage JWT
│           ├── encomendas/           # Câmera → OCR → Confirmação → WhatsApp
│           ├── moradores/            # (estrutura espelho, para implementar)
│           └── relatorios/           # (estrutura espelho, para implementar)
│
├── web/
│   └── cookie-banner.html            # Banner LGPD com localStorage + fallback cookie
│
└── legal/
    ├── politica-de-privacidade.md    # Política LGPD (modelo B2B2C / Operadora)
    └── termos-de-uso.md              # Termos de Uso com divisão de responsabilidade
```

---

## Stack Tecnológica

| Camada              | Tecnologia                                  | Justificativa                                                     |
| ------------------- | ------------------------------------------- | ----------------------------------------------------------------- |
| Banco de Dados      | PostgreSQL via **Supabase**                 | RLS nativo, Auth integrado, PostgREST como fallback               |
| Autenticação        | **Supabase Auth** + JWT                     | MFA, providers sociais, JWT com claims customizadas               |
| Backend             | **.NET 10 Web API**                         | Performance, tipagem forte, ecossistema maduro                    |
| Arquitetura Backend | **Clean Architecture** + **CQRS** (MediatR) | Isolamento de responsabilidades, testabilidade                    |
| ORM/Query           | **Dapper** + SQL nativo                     | Controle total das queries, performance máxima                    |
| Frontend Mobile     | **Flutter** + **BLoC**                      | Melhor integração nativa com câmera; BLoC garante separação limpa |
| Mensageria          | **Meta WhatsApp Cloud API**                 | 1.000 conversas/mês gratuitas por tenant (ideal para MVP)         |

---

## Padrões de Design Implementados

### State (Ciclo de Vida da Encomenda)

```
Pendente ──[Notificar]──► Notificado ──[Retirar + código correto]──► Retirada
    │                          │
    └──[Extraviar]──► Extraviado  └──[Extraviar]──► Extraviado
```

Arquivo: [backend/src/MeuCondominio.Domain/States/](backend/src/MeuCondominio.Domain/States/)

### Strategy + Factory (OCR de Etiquetas)

- `MercadoLivreOcrStrategy` — regex para `"Complemento: Bloco [X] Apto [Y]"`
- `AmazonOcrStrategy` — regex para `"Apt [Y] Blk [X]"`
- `GenericOcrStrategy` — fallback para padrões desconhecidos

Para adicionar um novo marketplace: crie uma nova `IEtiquetaOcrStrategy` e registre-a no `Program.cs`. Zero mudanças na Factory.

Arquivo: [backend/src/MeuCondominio.Application/OCR/](backend/src/MeuCondominio.Application/OCR/)

### Builder com Fluent API (Relatórios do Síndico)

```csharp
var filtros = new RelatorioEncomendaBuilder(condominioId)
    .ComPeriodo(inicio, fim)
    .ComStatus("Pendente")
    .ComUnidade("B", "304")
    .Build();
```

Arquivo: [backend/src/MeuCondominio.Application/Relatorios/RelatorioEncomendaBuilder.cs](backend/src/MeuCondominio.Application/Relatorios/RelatorioEncomendaBuilder.cs)

---

## Segurança Multi-Tenant

1. **Row Level Security (RLS)** no PostgreSQL: toda query é automaticamente filtrada pelo `condominio_id` do usuário logado — implementado nas funções `get_condominio_id()` e `get_user_role()`.
2. **JWT Claims**: o `condominio_id` é injetado como claim no token e validado em cada request pelo backend.
3. **Credenciais WhatsApp por Tenant**: tokens Meta ficam na tabela `whatsapp_config` e são buscados dinamicamente. Zero hardcode.
4. **Mascaramento LGPD na UI**: `EncomendaModel.nomeMascarado` e ofuscação de WhatsApp são aplicados antes de renderizar na tela da portaria.

---

## Como Adicionar um Novo Módulo (ex: Avisos)

Seguindo o **Open/Closed Principle**, nenhum arquivo existente precisa ser modificado:

**Backend:**

1. Crie `AvisoEntity.cs` em `Domain/Entities/`
2. Crie `AvisarMoradores/AvisarCommand.cs` e `AvisarHandler.cs` em `Application/UseCases/Avisos/`
3. Crie `AvisosController.cs` em `API/Controllers/`

**Mobile:**

1. Crie a pasta `features/avisos/` com `data/`, `domain/` e `presentation/bloc/` + `pages/`
2. Adicione a rota em `AppRouter`

---

## Setup Rápido

### 1. Banco de Dados

Execute [database/schema.sql](database/schema.sql) no **SQL Editor do Supabase**.

### 2. Backend

```powershell
cd backend/src/MeuCondominio.API
# Configure appsettings.json com Connection String e JwtSecret do Supabase
# As migrações SQL são aplicadas automaticamente ao iniciar (DbUp)
dotnet run
```

### 3. Mobile

```bash
cd mobile
flutter pub get
# Configure lib/core/api/api_client.dart com a URL da sua API
flutter run
```

---

## Compliance LGPD

| Entregável                      | Localização                                                                                                                                                                          |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Política de Privacidade         | [legal/politica-de-privacidade.md](legal/politica-de-privacidade.md)                                                                                                                 |
| Termos de Uso                   | [legal/termos-de-uso.md](legal/termos-de-uso.md)                                                                                                                                     |
| Banner de Cookies (HTML/CSS/JS) | [web/cookie-banner.html](web/cookie-banner.html)                                                                                                                                     |
| RLS no Banco                    | [backend/src/MeuCondominio.Infrastructure/Persistence/Migrations/V001\_\_InitialSchema.sql](backend/src/MeuCondominio.Infrastructure/Persistence/Migrations/V001__InitialSchema.sql) |

---

> **Aviso:** Os documentos jurídicos são minutas de referência. Consulte um advogado antes de publicá-los.
