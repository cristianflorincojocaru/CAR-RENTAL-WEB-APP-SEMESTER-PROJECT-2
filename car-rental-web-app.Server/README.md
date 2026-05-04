# WheelDeal Car Rental — Backend API

ASP.NET Core 8 REST API using Clean Architecture, Entity Framework Core, SQL Server and JWT authentication.

---

## Project Structure

```
CarRental/
├── Domain/                         # Enterprise business rules
│   ├── Entities/                   # User, Vehicle, Client, Rental, Branch, ...
│   └── Interfaces/                 # IRepository, IUnitOfWork contracts
│
├── Application/                    # Application business rules
│   ├── DTOs/                       # Request/Response objects
│   ├── Interfaces/                 # IAuthService, IVehicleService, ...
│   ├── Mappings/                   # Entity → DTO mapping extensions
│   └── Services/                   # AuthService, VehicleService, RentalService, ...
│
├── Infrastructure/                 # Framework & external concerns
│   ├── Data/
│   │   ├── AppDbContext.cs         # EF Core DbContext + Fluent API config
│   │   ├── UnitOfWork.cs           # UoW pattern
│   │   ├── DbSeeder.cs             # Initial seed data
│   │   ├── DesignTimeDbContextFactory.cs
│   │   └── Migrations/             # EF Core migrations
│   ├── Repositories/               # All repository implementations
│   └── Services/                   # JwtService
│
└── API/                            # Presentation layer
    ├── Controllers/                # AuthController, VehiclesController, ...
    ├── Extensions/                 # ServiceExtensions (DI registration)
    ├── Middleware/                 # ExceptionMiddleware
    ├── Properties/launchSettings.json
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Program.cs
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio) **or** any SQL Server instance
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension

---

## Quick Start

### 1. Clone / copy the solution

Place the `CarRental/` folder anywhere on your machine.

### 2. Update the connection string (optional)

Edit `API/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WheelDealDb_Dev;Trusted_Connection=True;TrustServerCertificate=True"
}
```

For a full SQL Server instance replace with:

```
Server=YOUR_SERVER;Database=WheelDealDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True
```

### 3. Apply migrations & seed

**Option A — via dotnet ef (recommended)**

```bash
cd CarRental

# Restore packages
dotnet restore

# Apply migration (creates DB + all tables)
dotnet ef database update --project Infrastructure --startup-project API

# The DbSeeder runs automatically on first startup
```

**Option B — raw SQL**

Run `Infrastructure/Data/Migrations/InitialCreate.sql` against your SQL Server instance using SSMS or sqlcmd.

### 4. Run the API

```bash
cd CarRental/API
dotnet run
```

Or press **F5** in Visual Studio (select the `https` profile).

The API will be available at:
- **https://localhost:7273**
- **http://localhost:5126**

Swagger UI: **https://localhost:7273/swagger**

---

## Default Credentials (seeded)

| Role          | Email                        | Password       |
|---------------|------------------------------|----------------|
| Administrator | admin@wheeldeal.ro           | Admin@123!     |
| Manager (BUC) | manager.buc@wheeldeal.ro     | Manager@123!   |
| Manager (CLJ) | manager.clj@wheeldeal.ro     | Manager@123!   |
| Manager (TIM) | manager.tim@wheeldeal.ro     | Manager@123!   |
| Operator      | op1.buc@wheeldeal.ro         | Operator@123!  |
| Operator      | op1.oto@wheeldeal.ro         | Operator@123!  |

---

## API Endpoints Summary

### Auth — `/api/auth`
| Method | Path                    | Auth     | Description               |
|--------|-------------------------|----------|---------------------------|
| POST   | `/login`                | Public   | Login, get JWT + refresh  |
| POST   | `/register`             | Public   | Register new account      |
| POST   | `/refresh`              | Public   | Rotate access token       |
| POST   | `/logout`               | Public   | Revoke refresh token      |
| POST   | `/change-password`      | Bearer   | Change own password       |
| GET    | `/me`                   | Bearer   | Get current user info     |

### Vehicles — `/api/vehicles`
| Method | Path               | Auth               | Description                     |
|--------|--------------------|--------------------|----------------------------------|
| GET    | `/`                | Public             | List / filter vehicles           |
| GET    | `/{id}`            | Public             | Get vehicle details              |
| GET    | `/branches`        | Public             | List branch names                |
| POST   | `/`                | Manager / Admin    | Add vehicle                      |
| PUT    | `/{id}`            | Manager / Admin    | Update vehicle                   |
| DELETE | `/{id}`            | Manager / Admin    | Deactivate vehicle               |
| PATCH  | `/{id}/offer`      | Manager / Admin    | Set / remove offer               |

### Clients — `/api/clients`
| Method | Path               | Auth               | Description                     |
|--------|--------------------|--------------------|----------------------------------|
| GET    | `/`                | Operator+          | List / search clients            |
| GET    | `/{id}`            | Operator+          | Get client                       |
| POST   | `/`                | Operator+          | Create client                    |
| PUT    | `/{id}`            | Manager+           | Update client                    |
| POST   | `/{id}/flag`       | Manager+           | Flag client                      |
| DELETE | `/{id}/flag`       | Manager+           | Remove flag                      |
| GET    | `/{id}/rentals`    | Operator+          | Client rental history            |

### Rentals — `/api/rentals`
| Method | Path               | Auth               | Description                     |
|--------|--------------------|--------------------|----------------------------------|
| GET    | `/`                | Operator+          | List / filter rentals            |
| GET    | `/{id}`            | Operator+          | Get rental details               |
| POST   | `/`                | Operator+          | Create rental contract           |
| PATCH  | `/{id}/complete`   | Operator+          | Mark as completed                |
| PATCH  | `/{id}/cancel`     | Manager+           | Cancel rental                    |
| GET    | `/my`              | Bearer             | Own rental history (client)      |

### Branches — `/api/branches`
| Method | Path                      | Auth     | Description          |
|--------|---------------------------|----------|----------------------|
| GET    | `/`                       | Public   | List active branches |
| GET    | `/{id}`                   | Public   | Get branch           |
| POST   | `/`                       | Admin    | Create branch        |
| PUT    | `/{id}`                   | Admin    | Update branch        |
| PATCH  | `/{id}/assign-manager`    | Admin    | Assign manager       |
| DELETE | `/{id}`                   | Admin    | Deactivate branch    |

### Users / Staff — `/api/users`
| Method | Path              | Auth  | Description            |
|--------|-------------------|-------|------------------------|
| GET    | `/`               | Admin | List all staff         |
| GET    | `/{id}`           | Admin | Get user               |
| POST   | `/`               | Admin | Create staff member    |
| PUT    | `/{id}`           | Admin | Update staff           |
| POST   | `/{id}/lock`      | Admin | Lock account           |
| POST   | `/{id}/unlock`    | Admin | Unlock account         |
| DELETE | `/{id}`           | Admin | Deactivate user        |

### Reports — `/api/reports`
| Method | Path                          | Auth         | Description           |
|--------|-------------------------------|--------------|-----------------------|
| GET    | `/dashboard`                  | Manager+     | Dashboard stats       |
| GET    | `/revenue`                    | Manager+     | Revenue summary       |
| GET    | `/security-alerts`            | Admin        | Unread security alerts|
| PATCH  | `/security-alerts/{id}/read`  | Admin        | Mark alert read       |
| GET    | `/security-alerts/count`      | Admin        | Unread alert count    |

### Contact — `/api/contact`
| Method | Path  | Auth   | Description         |
|--------|-------|--------|---------------------|
| POST   | `/`   | Public | Submit contact form |

### Health — `/api/health`
| Method | Path  | Auth   | Description    |
|--------|-------|--------|----------------|
| GET    | `/`   | Public | Health check   |

---

## JWT Configuration

Tokens are configured in `appsettings.json`:

```json
"Jwt": {
  "Key":      "WheelDeal_SuperSecret_JWT_Key_2026_Must_Be_At_Least_32_Characters_Long!",
  "Issuer":   "WheelDeal.API",
  "Audience": "WheelDeal.Client"
}
```

- **Access token**: expires in 15 minutes
- **Refresh token**: expires in 7 days, stored in DB
- On 5 failed logins: account locks for 15 min + SecurityAlert created

---

## Angular Frontend Integration

Update `car-rental-web-app.client/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7273/api'
};
```

The Angular interceptors (`auth.interceptor.ts`, `refresh.interceptor.ts`) already handle Bearer tokens and automatic token refresh — no changes needed there.

---

## Role Values (stored as int in DB)

| Role          | Int |
|---------------|-----|
| Administrator | 0   |
| Manager       | 1   |
| Operator      | 2   |

## Vehicle Category Values

| Category | Int |
|----------|-----|
| Economy  | 0   |
| Compact  | 1   |
| SUV      | 2   |
| Premium  | 3   |
| Van      | 4   |

## Rental Status Values

| Status    | Int |
|-----------|-----|
| Active    | 0   |
| Completed | 1   |
| Cancelled | 2   |

---

## Adding a New Migration

After changing any Domain entity:

```bash
cd CarRental
dotnet ef migrations add YourMigrationName --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
```

---

## Security Notes

- **Change the JWT Key** in production — use `dotnet user-secrets` or environment variables
- **Never commit** `appsettings.Development.json` with real credentials
- Refresh tokens are single-use (rotated on each refresh)
- All refresh tokens are revoked on password change

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `Cannot connect to LocalDB` | Ensure SQL Server LocalDB is installed (ships with Visual Studio) |
| `Invalid JWT Key` | Key must be ≥ 32 characters |
| `CORS error from Angular` | Ensure Angular runs on `localhost:4200`; check CORS policy in `ServiceExtensions.cs` |
| `Migration not found` | Run `dotnet ef database update` from solution root, pointing to Infrastructure project |
