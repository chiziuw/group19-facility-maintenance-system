# Group 19 — Facility Maintenance Request System (C# .NET 8)

A layered, Object-Oriented C# application that allows facility users to report faulty equipment or facilities, categorize and prioritize requests, assign maintenance technicians, and track complete repair lifecycles with persistent JSON storage.

## Team Members (Group 19)
- **Uwadileke Chizaram** — Group Leader / Project Manager & Lead Backend C# Developer
- **Emmanuel Chinonye** — System Modeller & UI/Frontend Developer
- **Igwagu Chidi** — Requirements Analyst, QA / Software Tester & Documentation Lead

## Architecture
- `Models/` — Domain entities (`User`, `Reporter`, `Technician`, `FacilityAdmin`, `MaintenanceRequest`, `StatusLogEntry`, `Enums`)
- `Data/` — Generic `IRepository<T>` interface and JSON persistence (`JsonRequestRepository`, `JsonUserRepository`)
- `Services/` — Business logic & validation rules (`AuthService`, `MaintenanceService`)
- `UI/` — Interactive role-based console dashboard (`ConsoleMenuApp`, `ConsoleHelper`)
- `FacilityMaintenanceSystem.Tests/` — Automated xUnit unit & integration tests

## Quick Start
Requires [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
# Run automated unit & integration tests
dotnet test

# Run the application
dotnet run --project FacilityMaintenanceSystem
```

## Demo Accounts (Password: `password`)
- **Facility Admin:** `admin`
- **Technicians:** `tech1`, `tech2`
- **Reporter:** `user1`
