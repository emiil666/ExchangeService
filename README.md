# Currency Exchange Office System

**Course:** Network Application Development
**Project title:** Currency Exchange Office System
**Author:** Emil Allahverdiyev
**Student ID:** 66258

## Description

A network-based application that simulates an online currency exchange office (kantor),
built on the .NET platform. It consists of three components:

- **WCF Web Service** – fetches live exchange rates from the National Bank of Poland (NBP)
  public API and performs currency conversion, buy/sell operations (with an office margin),
  historical-rate lookups, and all user-account and transaction logic.
- **WPF Client Application** – a desktop interface that communicates with the web service.
  Users can convert currencies, view historical rates, register or log in, top up a PLN
  balance, buy and sell foreign currencies, and view their transaction history.
- **SQL Server LocalDB Database** – stores user accounts, their PLN balances, and a log of
  every currency transaction.

## Technologies

- .NET Framework (C#)
- Windows Communication Foundation (WCF)
- Windows Presentation Foundation (WPF)
- SQL Server LocalDB (ADO.NET)
- NBP public API (https://api.nbp.pl)

## How to Run

1. Open `ExchangeService.slnx` in Visual Studio (with the **.NET desktop development** and
   **Windows Communication Foundation** components installed).
2. Create a SQL Server LocalDB database named `ExchangeDB`, then run
   `Database/database_setup.sql` once to create the `Users` and `Transactions` tables.
3. If running on a different machine, update the connection string in
   `ExchangeService/Service1.svc.cs` to match your LocalDB instance
   (for example `(localdb)\MSSQLLocalDB`).
4. In Visual Studio, open **Configure Startup Projects** and set both projects to start,
   with the service first, then the client.
5. Press **F5**. The web service starts and the client window opens.

> Note: the solution uses the newer `.slnx` solution format (Visual Studio 2022 17.13+/2026).

## Repository Structure

- `ExchangeService/` – WCF web service (business logic, NBP integration, database access)
- `ExchangeClient/` – WPF client application
- `Database/` – SQL setup script
- `Documentation/` – project documentation
