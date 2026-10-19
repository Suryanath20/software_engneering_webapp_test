# ANDtr — Medical Device Safety and Recall Notice Platform

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Database](https://img.shields.io/badge/Database-MySQL%208.0-orange.svg)](https://www.mysql.com/)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20Passing-brightgreen.svg)]()

> Enterprise web platform for the systematic ingestion, risk review, multi-stakeholder collaboration, and customer distribution of FDA Medical Device Recalls.

---

## 📋 Table of Contents
1. [Platform Overview](#platform-overview)
2. [Team Contributions & Feature Ownership](#team-contributions--feature-ownership)
3. [Architecture & Key Technologies](#architecture--key-technologies)
4. [6-Step Quickstart Guide](#6-step-quickstart-guide)
5. [Default User Credentials](#default-user-credentials)
6. [Branching & Git Workflow](#branching--git-workflow)
7. [Running Automated Tests](#running-automated-tests)
8. [Documentation Links](#documentation-links)

---

## 🏥 Platform Overview
**ANDtr** serves regulatory compliance officers, clinical safety engineers, hospital procurement teams, and healthcare executives by turning high-volume FDA medical device recall notices into actionable, auditable risk assessments. 

Key capabilities include:
- **High-Throughput Excel Ingestion:** Parse multi-hundred row FDA datasets with automatic duplicate detection, data normalization, and carriage-return sanitization.
- **Parametric Notice Search & Synonyms:** Fast querying with keyword search, classification filters (Class I, II, III), firm filters, and medical synonym enrichment.
- **Cross-Page Notice Selection:** Persistent notice basket across paginated search results to seed new risk reviews.
- **Structured Risk Reviews & Findings:** Rich-text clinical impact assessments, risk ratings (Critical, High, Moderate, Low), and executive summaries.
- **Multi-Party Sharing:** Secure review distribution to authorized customer organizations with access revocation and access logs.
- **Two-Way Customer Feedback:** Interactive comment threads between hospital representatives and ANDtr compliance officers.
- **Executive PDF & CSV Reports:** Formatted printable clinical review reports and spreadsheet exports.

---

## 👥 Team Contributions & Feature Ownership

The platform implements all user journeys and wireframe specifications across 5 team modules:

| Team Member | Wireframe Source | Feature Modules & Ownership | Git Branch Convention |
| :--- | :--- | :--- | :--- |
| **Suryanath Sunil** | `Suryanath_Sunil.html` | **Dashboard, Search & Filtering:** Operational KPIs, quick recall lookups, persistent cross-page checkbox selection, parametric filters (class, firm, status, date), customer account activation management. | `feature/suryanath-dashboard-search` |
| **Azlan Ahmed** | `Azlan_Ahmed.html` | **Data Ingestion & Notice Details:** ClosedXML Excel pipeline, duplicate recall checking, ingestion audit logs, full recall notice specification view, product identification. | `feature/azlan-notice-import` |
| **Hassan Naveed** | `Hassan_Naveed.html` | **Risk Reviews & Findings:** Review authoring studio, vanilla JS rich-text editor with HTML sanitization, risk scoring matrix, executive print/PDF reporting, CSV exports. | `feature/hassan-reviews-findings` |
| **Sanjida Afrin** | `Sanjida_Afrin.html` | **Collaboration & Distribution:** Customer sharing modal, active share management, instant revocation, access audit timestamps, review dispatch workflow. | `feature/sanjida-sharing-collaboration` |
| **Tamilore Adekanye** | `Tamilore_Adekanye.html` | **Customer Portal & Dialogue:** Customer-facing portal, shared review viewer, interactive comment thread with "awaiting response" state management. | `feature/tamilore-customer-portal` |

---

## ⚙️ Architecture & Key Technologies

- **Runtime & Framework:** ASP.NET Core 8.0 MVC (C# 12)
- **Data Persistence:** Entity Framework Core 8 with Pomelo MySQL 8 Provider (`8.0.2`), including transparent in-memory fallback for local environments without Docker.
- **Authentication & Security:** ASP.NET Core Identity with role-based authorization (`Employee`, `Customer`), password hashing, and anti-forgery token verification (`[ValidateAntiForgeryToken]`).
- **Excel Processing:** ClosedXML (`0.102.2`) for high-performance `.xlsx` streaming and workbook parsing.
- **HTML Sanitization:** Ganss.Xss.HtmlSanitizer (`9.2.1039`) preventing XSS in rich-text findings.
- **Frontend:** Server-rendered Razor views styled with Bootstrap 5, custom Crimson theme (`#C8102E`), vanilla JavaScript (no heavy node build dependencies), and print-optimized stylesheets (`@media print`).
- **Testing:** xUnit, Moq, EF Core InMemory, Coverlet test runner.

---

## 🚀 6-Step Quickstart Guide

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for MySQL 8 container) OR a local MySQL 8 instance

### 2. Clone Repository
```bash
cd ANDtr
```

### 3. Start Database Container
```bash
docker compose up -d
```
*Note: Verify container is healthy with `docker ps`. If running without Docker, `ANDtr.Web` will automatically use an in-memory database to allow immediate exploration.*

### 4. Build Solution
```bash
dotnet build
```

### 5. Run Application & Auto-Seed
```bash
dotnet run --project src/ANDtr.Web
```
*The application starts at `http://localhost:5000` (or `https://localhost:5001`).*
*On first startup, the database is automatically created, schema migrated, FDA sample dataset (616 notices) ingested, and test accounts/reviews seeded.*

### 6. Run Automated Tests
```bash
dotnet test
```

---

## 🔑 Default User Credentials

| Role | Username / Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Employee** (Compliance Officer) | `employee@andtr.test` | `Employee#123` | Full access: Ingestion, notice catalog, review creation, rich-text findings, sharing, customer management, reports. |
| **Customer** (Hospital Officer) | `customer@andtr.test` | `Customer#123` | Restricted access: Customer portal, shared reviews, two-way comment threads, executive report printing. |

---

## 🌿 Branching & Git Workflow

All development follows trunk-based feature branching:
```bash
# Example feature branch creation
git checkout -b feature/suryanath-dashboard-search
git checkout -b feature/azlan-notice-import
git checkout -b feature/hassan-reviews-findings
git checkout -b feature/sanjida-sharing-collaboration
git checkout -b feature/tamilore-customer-portal
```
*Pull requests require passing unit tests and code review before merging into `main`.*

---

## 🧪 Running Automated Tests

Run the full xUnit test suite covering search, import, review CRUD, sharing security, and comment threads:
```bash
dotnet test tests/ANDtr.Tests/ANDtr.Tests.csproj --logger "console;verbosity=detailed"
```

---

## 📚 Documentation Links
- [Architecture & Design Guide](docs/ARCHITECTURE.md)
- [Database ER Diagram & Schema](docs/ER_DIAGRAM.md)
- [Comprehensive Test Plan (20+ Cases)](docs/TEST_PLAN.md)
- [2-Year Maintenance & Scalability Plan](docs/MAINTENANCE.md)
