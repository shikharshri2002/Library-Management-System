# Shelfwise Library Management System

A full-stack library application built with Angular, ASP.NET Core, Entity Framework Core, JWT authentication, and SQL Server.

## Requirements

- .NET 10 SDK
- Node.js 20.19+ (Node 24 recommended) and npm
- SQL Server (Developer, Express, Standard, or another reachable SQL Server instance)
- Angular CLI is optional; the frontend npm scripts use the locally installed CLI

## Project layout

```text
Backend/LibraryApi/          ASP.NET Core REST API and EF Core models
Frontend/library-ui/         Angular standalone application
database/schema.sql          SQL Server schema reference
database/stored-procedures.sql
database/seed.sql            Optional sample catalog
```

## Configure and run

The default connection targets `LENOVOSLIMPAD-3\SQLEXPRESS` using Windows Authentication. Update `ConnectionStrings__LibraryDatabase` in `Backend/LibraryApi/appsettings.json` if your SQL Server instance is different. Create an empty `LibraryManagement` database in SQL Server Management Studio (SSMS) first, or let the API create it if the configured login has permission to create databases. At startup, EF Core applies the tables and stored procedure to that database.

For Windows Authentication, replace `YOUR_SERVER\\YOUR_INSTANCE` with the server and instance shown in SSMS under **Server name**:

```powershell
$env:ConnectionStrings__LibraryDatabase = "Server=YOUR_SERVER\YOUR_INSTANCE;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=True"
```

For SQL Server Authentication, keep the username and password out of source control and set the connection string in your terminal instead:

```powershell
$env:ConnectionStrings__LibraryDatabase = "Server=YOUR_SERVER\YOUR_INSTANCE;Database=LibraryManagement;User ID=YOUR_SQL_LOGIN;Encrypt=True;TrustServerCertificate=True"
$sqlPassword = Read-Host "SQL Server password"
$env:ConnectionStrings__LibraryDatabase += ";Pass" + "word=" + $sqlPassword
```

`TrustServerCertificate=True` is convenient for local development; use a trusted SQL Server certificate in production. The application login needs permission to connect to the database and create or update its schema when EF migrations run.

Set a private JWT signing key before deployment; the key in `appsettings.json` is for local development only.

In PowerShell, set up an administrator account before the first API run:

```powershell
$env:BootstrapAdmin__Email = "admin@example.com"
$env:BootstrapAdmin__Password = "choose-a-long-unique-password"
$env:Jwt__Key = "replace-with-a-private-secret-at-least-32-bytes-long"
dotnet run --project .\Backend\LibraryApi
```

The API applies EF Core migrations at startup. The bootstrap administrator is created only when the configured email does not already exist; public registration always creates a regular `User` account. After the administrator has been created, the bootstrap variables can be removed. Do not deploy with the development JWT key.

Run the API from the same PowerShell window after setting the SQL Server connection string, then run the Angular app in another PowerShell window:

```powershell
Set-Location .\Frontend\library-ui
npm start
```

The UI runs at `http://localhost:4200` or `http://127.0.0.1:4200`; the API's HTTP launch profile listens on `http://localhost:5160`. Add any other frontend origin to `AllowedOrigins` in API configuration. The frontend API URL is `http://localhost:5160/api`.

## Database setup and sample data

EF migrations create and update the application schema automatically on API startup. `database/schema.sql` documents the equivalent SQL Server schema for manual setup; do not run it against a database managed by the API migrations. The initial migration installs the overdue-books procedure automatically. To load the optional sample catalog into a database already created by the API, replace the server name below with your SQL Server instance:

```powershell
sqlcmd -S "YOUR_SERVER\YOUR_INSTANCE" -d LibraryManagement -E -i .\database\seed.sql
```


## Main API routes

| Method | Route | Access |
| --- | --- | --- |
| POST | `/api/auth/register` | Public; creates a User account and linked member |
| POST | `/api/auth/login` | Public |
| GET | `/api/books?search=` | Authenticated |
| POST, PUT, DELETE | `/api/books` | Admin |
| GET, POST, PUT, DELETE | `/api/members` | Admin |
| GET, POST | `/api/borrowing` | Authenticated; users can issue for themselves |
| POST | `/api/borrowing/{id}/return` | Record owner or Admin |
| GET | `/api/borrowing/overdue` | Admin; calls `sp_GetOverdueBooks` |

## Features

- BCrypt password hashing, signed expiring JWTs, and Admin/User role authorization.
- Role-protected Angular routes and bearer-token HTTP interceptor.
- Searchable book catalog, member management, checkout, return, and overdue report.
- Inventory updates on checkout/return, copy-count validation, and borrowing-history-safe deletion.
- SQL Server constraints and stored-procedure-backed overdue tracking.
