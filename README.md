# beeday Frontend Lab

An independently runnable, **database-free** Blazor Server workspace for visual development and
owner review of the beeday Experience System.

This repository is part of **EPIC 33** (`DS-Asyst/BeeDay`, Issue
[#361](https://github.com/DS-Asyst/BeeDay/issues/361)). Its architecture, boundaries, and the
COPY/ADAPT/MOCK/EXCLUDE extraction rule it follows are defined in
[`ADR-008`](https://github.com/DS-Asyst/BeeDay/blob/hmg/docs/adr/ADR-008-frontend-lab-architecture-boundaries.md)
in the `DS-Asyst/BeeDay` repository. Read that document before adding anything here.

## What this is — and is not

`DS-Asyst/BeeDay` remains the only runtime, business, and production source of truth. This
repository never becomes a second backend.

Allowed: Blazor/Razor presentation components, CSS/design tokens, layouts, static/local assets,
presentation-only view models, deterministic scenario providers, component/page/email galleries,
localization resources needed for visual parity, preview-only transactional e-mail HTML,
accessibility helpers, and code/component/source-contract tests.

Never allowed here: `BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`, EF Core, SQL
Server, SQL Server LocalDB, migrations, repositories/Unit of Work, real authentication/session
infrastructure, real Resend/SMTP e-mail delivery, API keys or production secrets, IIS production
deployment code, or a production business workflow reimplemented as mock logic. This is enforced in
code by `tests/BeeDayLab.ArchitectureTests`, not just by convention.

## Branches

- `hmg` — active integration branch. All Sprint/feature/fix branches target this.
- `prd` — protected, validated visual source eligible for controlled promotion into
  `DS-Asyst/BeeDay`. Never means "deployed production."

No direct push to either branch — every change goes through a pull request and the required `Lab CI`
check.

## Local development

Requirements: .NET 10 SDK. No database, no secrets, no external service.

```bash
dotnet restore BeeDayLab.slnx
dotnet run --project src/BeeDayLab.Web
```

Validate before opening a PR:

```bash
dotnet format BeeDayLab.slnx --verify-no-changes
dotnet build BeeDayLab.slnx --configuration Release --warnaserror
dotnet test BeeDayLab.slnx --configuration Release
```

`dotnet test BeeDayLab.slnx` is safe to run unrestricted here — unlike `DS-Asyst/BeeDay`, this
solution is guaranteed database-free by `BeeDayLab.ArchitectureTests`, so there is no LocalDB/E2E
project to accidentally trigger.

## Structure

```text
src/BeeDayLab.Web/            Blazor Server app — no ProjectReference to any BeeDay.* project
tests/BeeDayLab.ArchitectureTests/   guards the boundary above in code
tests/BeeDayLab.Web.Tests/           bUnit component tests
```

This is the empty bootstrap shell (Sprint 33.5). Extraction of the real Design System foundations,
components, layouts, pages, and e-mail templates follows the Sprint 33.6–33.15 plan and the
canonical `FE33-*` Ledger in `DS-Asyst/BeeDay`
(`docs/epics/33-ds-assyst-frontend-lab/03-frontend-inventory-ledger.md`).

## Promotion

A `prd` revision is never blindly synchronized into `DS-Asyst/BeeDay`. See the Promotion & Drift
Contract referenced by ADR-008 for the full, deliberate integration flow.
