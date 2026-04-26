# IncTrak Options

**An employee stock-option management tool for startups and growing teams.**  
IncTrak helps you manage stock classes, grants, vesting schedules, participants, and optionee views without spreadsheets.

---

## Table of Contents
- [Overview](#overview)  
- [Key Features](#key-features)  
- [Quick Start](#quick-start)  
- [Core Concepts](#core-concepts)  
- [Screens & Navigation](#screens--navigation)  
- [Configuration](#configuration)  
- [Development](#development)  
- [FAQ / Help](#faq--help)  
- [Roadmap](#roadmap)  
- [License](#license)  
- [Contact](#contact)

---

## Overview

IncTrak provides an intuitive, always-accessible experience for both administrators and participants to view and manage equity plans with clear vesting visuals and secure access control.

- [Website](https://www.inctrak.com/)  
- [Documentation](https://docs.inctrak.com/)  
- [Shared Portal](https://shared.inctrak.com/)  
- [Blog](https://blog.inctrak.com/)  

## Development

The API lives in `shared.inctrak.com/`. The frontend source lives in `frontend/` as a Vue 3 + TypeScript + Vite SPA, and the `inctrak.com/`, `docs.inctrak.com/`, and `blog.inctrak.com/` folders remain plain static sites.

- `./build.sh` builds and tests the frontend, then builds the API project and its test project from the repo root.
- `npm install --prefix frontend` installs frontend dependencies.
- `npm run dev --prefix frontend` starts the Vite dev server.
- `npm run build --prefix frontend` produces the generated frontend bundle in `frontend/dist/`.
- `npm run test --prefix frontend` runs the frontend unit tests.
- `dotnet build shared.inctrak.com/shared.inctrak.com.csproj` builds the API directly.
- `dotnet run --project shared.inctrak.com/shared.inctrak.com.csproj` starts the API locally.
- `dotnet test shared.inctrak.com.Tests/shared.inctrak.com.Tests.csproj` runs the API split tests.

For local SPA work, run the Vite dev server from `frontend/`. The SPA now calls relative `/api/*` paths, and Vite proxies those requests to `VITE_API_PROXY_TARGET`, which defaults to `http://localhost:5000`.

For env-driven local backend runs, copy `scripts/inctrak/config.example.yaml` to `scripts/inctrak/config.local.yaml`, then use the VS Code launch flow documented in [docs/inctrak_local_vscode.md](docs/inctrak_local_vscode.md). For Cloudflare Pages proxy setup, see [docs/cloudflare-pages-gateway.md](docs/cloudflare-pages-gateway.md).

---

## Key Features

- **Company administration**: Define stock classes, stock holders, plans, vesting schedules, termination dates, participants, and grants.  
- **Participant/Optionee views**: Personal stock and option summaries with grant details and vesting timelines.  
- **Simple startup onboarding**: Quick setup with clear “Getting Started” docs, FAQs, showcases, and training videos.  
- **Accessible anywhere**: Cloud-based experience designed to replace error-prone spreadsheets.  

---

## Quick Start

The docs include a **Quick Start** guide that shows how to view vested options and set up user access.  
High-level steps:

1. Create your company and base configuration (stock classes, plans).  
2. Add participants and create grants (ISO/NSO as appropriate).  
3. Define vesting schedules (with optional cliffs and termination dates).  
4. Invite users so administrators and participants can access their dashboards.  

See the [Quick Start Guide](https://docs.inctrak.com/) for full details.

---

## Core Concepts

- **Stock Classes & Plans** — Model your capitalization structure and option plans.  
- **Participants & Grants** — Assign grants to employees/consultants and track details.  
- **Vesting Schedules** — Define time-based vesting with optional cliffs and termination handling.  
- **Admin vs. Optionee** — Role-appropriate views and permissions for clear separation of duties.  

---

## Screens & Navigation

From the shared portal:  

- **Company**: Stock Classes, Stock Holders, Plans, Vesting Schedules, Termination Dates, Participants, Grants  
- **Optionee**: Stock Summary, Option Summary, Grants  
- **Etc.**: About, Contact, Login/Register, Reset Password, Privacy Policy, Docs  

---
