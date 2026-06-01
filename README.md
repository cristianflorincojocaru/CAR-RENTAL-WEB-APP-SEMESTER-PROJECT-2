# Semester Project No. 2 - CAR RENTAL COMPANY WEBSITE

<div align="center">

**🚗 WHEELDEAL — a full-stack car rental platform built with Angular + ASP.NET Core 10 + MS SQL Server**

![Angular](https://img.shields.io/badge/Angular-Standalone-DD0031?style=for-the-badge&logo=angular)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-512BD4?style=for-the-badge&logo=dotnet)
![MS SQL](https://img.shields.io/badge/MS%20SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=dotnet)
![Playwright](https://img.shields.io/badge/Playwright-E2E%20Testing-45ba4b?style=for-the-badge&logo=playwright)

</div>


## OVERVIEW

**WheelDeal** is a production-quality car rental platform founded in 2016, headquartered in Bucharest, Romania. The application covers the complete rental lifecycle — browsing vehicles, checking availability, making bookings, applying promo codes, submitting contact requests, and administering the fleet and clients through a full dashboard — built with a clean separation of concerns across four layers: API, Application, Domain, and Infrastructure.

### DESIGN LANGUAGE

Modern automotive aesthetic: dark navy primary palette, electric blue accents (`var(--color-primary)`), consistent `fadeInUp` card animations, and two reusable button interaction patterns used across all pages — a `btn--reactive` lift style (Book Now) and a ghost ring style (Sign In / View All Cars). The design is fully responsive with a dedicated mobile layout.


## ARCHITECTURE

### BACKEND — `car-rental-web-app.Server/`

```
car-rental-web-app.Server/
├── CarRental.sln
│
├── API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ClientsRentalsController.cs
│   │   ├── OtherControllers.cs
│   │   └── VehiclesController.cs
│   ├── Extensions/
│   │   └── ServiceExtensions.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
├── Application/
│   ├── DTOs/
│   │   ├── Auth/       AuthDtos.cs
│   │   ├── Branches/   BranchDtos.cs
│   │   ├── Clients/    ClientDtos.cs
│   │   ├── Contact/    ContactDtos.cs
│   │   ├── Promo/      PromoDtos.cs
│   │   ├── Rentals/    RentalDtos.cs
│   │   ├── Reports/    ReportDtos.cs
│   │   └── Vehicles/   VehicleDtos.cs
│   ├── Interfaces/
│   │   └── IServices.cs
│   ├── Mappings/
│   │   └── MappingExtensions.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── ClientService.cs
│       ├── OtherServices.cs
│       ├── RentalService.cs
│       └── VehicleService.cs
│
├── Domain/
│   ├── Entities/
│   │   ├── Branch.cs
│   │   ├── Client.cs
│   │   ├── Enums.cs
│   │   ├── Rental.cs
│   │   ├── SupportEntities.cs
│   │   ├── User.cs
│   │   └── Vehicle.cs
│   └── Interfaces/
│       └── IRepositories.cs
│
└── Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   ├── DbSeeder.cs
    │   ├── DesignTimeDbContextFactory.cs
    │   └── UnitOfWork.cs
    ├── Migrations/
    │   ├── 20260508213855_InitialCreate.cs
    │   ├── 20260509124857_AddContactMessages.cs
    │   ├── 20260509200004_AddPromoCodes.cs
    │   ├── 20260509205420_AddVehicleImageUrl.cs
    │   └── AppDbContextModelSnapshot.cs
    ├── Repositories/
    │   └── Repositories.cs
    └── Services/
        └── JwtService.cs
```

### FRONTEND — `car-rental-web-app.client/`

```
car-rental-web-app.client/
├── angular.json
├── package.json
├── playwright.config.ts
├── wheeldeal.spec.ts
├── tsconfig.json
├── .editorconfig
├── .prettierrc
│
├── public/
│   └── favicon.ico
│
└── src/
    ├── index.html
    ├── main.ts
    ├── styles.scss
    │
    ├── environments/
    │   ├── environment.ts
    │   └── environment.prod.ts
    │
    └── app/
        ├── app.config.ts
        ├── app.routes.ts
        ├── app.ts / app.html / app.scss
        │
        ├── guards/
        │   └── auth.guard.ts
        │
        ├── interceptors/
        │   ├── auth.interceptor.ts
        │   └── refresh.interceptor.ts
        │
        ├── models/
        │   ├── api-error.model.ts
        │   ├── auth.models.ts
        │   ├── booking.models.ts
        │   ├── car.models.ts
        │   └── contact.models.ts
        │
        ├── pages/
        │   ├── about us/       about.ts / about.html / about.scss
        │   ├── auth/
        │   │   ├── login/      login.ts / login.html / login.scss
        │   │   └── signup/     signup.ts / signup.html / signup.scss
        │   ├── booking/        booking.ts / booking.html / booking.scss
        │   ├── cars/           cars.ts / cars.html / cars.scss
        │   ├── contact/        contact.ts / contact.html / contact.scss
        │   ├── dashboard/      dashboard.ts / dashboard.html / dashboard.scss
        │   ├── home/           home.ts / home.html / home.scss
        │   ├── my-bookings/    my-bookings.ts / my-bookings.html / my-bookings.scss
        │   └── offers/         offers.ts / offers.html / offers.scss
        │
        ├── services/
        │   ├── auth.service.ts
        │   ├── booking.service.ts
        │   ├── car.service.ts
        │   ├── contact.service.ts
        │   └── token.service.ts
        │
        └── shared/
            └── navbar/         navbar.ts / navbar.html / navbar.scss
```

### FLEET ASSETS — `src/assets/cars/`

The vehicle image library is organized by budget tier. Each vehicle has 3 color variants × 2 angles = 6 images, stored under assets/cars/{tier}/{make_model}/updated/. The fleet covers 46 vehicles across 5 tiers.

| TIER | VEHICLES |
|---|---|
| **LOW BUDGET** | Dacia Logan, Dacia Sandero, Fiat 500, Hyundai i20, Kia Picanto, MG3, Mitsubishi Space Star, Renault Clio, Skoda Fabia, Suzuki Swift |
| **MEDIUM BUDGET** | Honda Civic, Hyundai Tucson, Kia Sportage, Mazda CX-5, Renault Austral, Skoda Octavia, Tesla Model 3, Toyota Corolla, Volkswagen Golf, Volvo XC40 |
| **HIGH BUDGET** | Alfa Romeo Stelvio, Audi Q7, BMW X5, Genesis GV80, Land Rover Defender 110, Lexus RX, Mercedes-Benz GLE, Porsche Macan, Tesla Model S, Volvo XC90 |
| **ELITE BUDGET** | Aston Martin DB12, Audi RS Q8, Bentley Continental GT, BMW XM, Ferrari Purosangue, Lamborghini Urus Performante, Mercedes-Benz G63 AMG, Porsche 911 Turbo S, Range Rover SV, Rolls-Royce Cullinan |
| **VANS** | Hyundai Staria, Lexus LM, Mercedes-Benz V-Class, Toyota Alphard, Volkswagen Multivan T7, Volvo EM90 |


## TECH STACK

| LAYER | TECHNOLOGY |
|---|---|
| Frontend | Angular (standalone components, CommonModule, FormsModule, RouterModule) |
| Backend | ASP.NET Core (.NET 10) |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Auth | JWT Bearer tokens + ASP.NET Core Identity |
| Token Handling | HTTP interceptors (auth + refresh) |
| API Docs | Scalar / OpenAPI |
| E2E Testing | Playwright |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API) |


## FEATURES

### 🔐 AUTHENTICATION
- Register / Login with JWT Bearer tokens
- Password hashing via ASP.NET Core Identity
- Role-based access control (Client / Admin / Staff)
- Forgot password flow with modal UI
- Global `auth.interceptor.ts` attaches tokens to every request
- `refresh.interceptor.ts` handles token refresh silently
- `AuthGuard` protects client and admin routes

### 🚗 FLEET / VEHICLES
- Browse and filter the full vehicle fleet across five budget tiers
- Category-based filtering with animated card transitions (`fadeInUp`)
- Per-card `trackBy` optimization for Angular rendering
- Three color variants and two angles per vehicle — 46 vehicles total
- Vehicle image URLs stored and served from the database

### 📅 BOOKINGS
- Full booking flow with car selection, date range picker (start + end date), and location input
- Short-term and long-term rental presets from the home search form
- My Bookings page with active / completed / cancelled filter tabs
- Client-linked rental history

### 🏷️ PROMO CODES
- Promo code validation and application at checkout
- Copy-to-clipboard button with `pointer-events: none` on child elements to prevent double-click conflicts
- Visual feedback on copy success

### 📬 CONTACT
- Full contact form with branch selector
- Real-time open/closed branch status computed via `Europe/Bucharest` timezone
- Four branch locations: Bucharest Central, Bucharest Otopeni, Cluj Airport, Timișoara Airport
- 10-second post-submission cooldown with countdown
- Social links block

### 🖥️ ADMIN DASHBOARD
- Overview with stat cards and quick actions
- Collapsible sidebar navigation
- **Vehicles tab** — full fleet table with search, row hover, and CRUD
- **Rentals tab** — filterable by Active / Completed / Cancelled status
- **Clients tab** — client listing with search
- **Staff tab** — staff management
- **Branches tab** — branch cards with hover states
- **Security Alerts** — system event monitoring
- **Revenue** — charts and revenue analytics
- **Report Builder** — configurable report sections with generate action

### 🌍 BRANCHES
- Branch entity with location, hours, and real-time open/closed status computed via `Europe/Bucharest` timezone
- Branch selector on the Contact page and in the admin Dashboard

### 📊 REPORTS
- Report DTOs for rental and fleet analytics
- Configurable report builder in the admin dashboard


## DATABASE SCHEMA

### ENTITIES

| ENTITY | KEY FIELDS |
|---|---|
| `User` | `Id`, `Email`, `PasswordHash`, `Role` |
| `Client` | `Id`, `UserId`, `FirstName`, `LastName`, `Phone`, `LicenseNumber` |
| `Vehicle` | `Id`, `Make`, `Model`, `Year`, `Category`, `PricePerDay`, `ImageUrl`, `BranchId` |
| `Branch` | `Id`, `Name`, `Location`, `Phone`, `OpenHour`, `CloseHour` |
| `Rental` | `Id`, `ClientId`, `VehicleId`, `StartDate`, `EndDate`, `TotalPrice`, `Status` |
| `PromoCode` | `Id`, `Code`, `DiscountPercent`, `ValidFrom`, `ValidTo`, `IsActive` |
| `ContactMessage` | `Id`, `Name`, `Email`, `BranchId`, `Message`, `SentAt` |

### RELATIONSHIPS

```
User ──< Client ──< Rental >── Vehicle >── Branch
                                  │
                             PromoCode (applied at checkout)

Branch ──< ContactMessage
```

### MIGRATIONS

```
20260508213855_InitialCreate
20260509124857_AddContactMessages
20260509200004_AddPromoCodes
20260509205420_AddVehicleImageUrl
```


## API ENDPOINTS

### AUTH
| METHOD | ENDPOINT | ACCESS |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |

### VEHICLES
| METHOD | ENDPOINT | ACCESS |
|---|---|---|
| GET | `/api/vehicles` | Public |
| GET | `/api/vehicles/{id}` | Public |
| POST | `/api/vehicles` | Admin |
| PUT | `/api/vehicles/{id}` | Admin |
| DELETE | `/api/vehicles/{id}` | Admin |

### RENTALS
| METHOD | ENDPOINT | ACCESS |
|---|---|---|
| GET | `/api/clients/{id}/rentals` | Client |
| POST | `/api/rentals` | Client |
| PUT | `/api/rentals/{id}` | Client / Admin |

### OTHER
| METHOD | ENDPOINT | ACCESS |
|---|---|---|
| GET | `/api/branches` | Public |
| POST | `/api/contact` | Public |
| POST | `/api/promo/validate` | Client |
| GET | `/api/reports` | Admin |


## FRONTEND PAGES

| ROUTE | PAGE | DESCRIPTION |
|---|---|---|
| `/` | Home | Hero search form, Featured Cars section, footer CTA |
| `/offers` | Offers | Promotional deals with ribbon badges and promo code input |
| `/cars` | Fleet | Full vehicle listing with budget-tier category filter |
| `/booking` | Booking | Car selection, date range picker, booking confirmation flow |
| `/my-bookings` | My Bookings | Rental history filtered by Active / Completed / Cancelled |
| `/about` | About | Company history (2016–2024), team bios, perks section |
| `/contact` | Contact | Branch selector with real-time open/closed status, contact form |
| `/login` | Login | Login form, register link, forgot password modal |
| `/signup` | Sign Up | Registration form |
| `/dashboard` | Dashboard | Full admin back-office — fleet, rentals, clients, reports |


## TESTING

E2E tests are written with **Playwright** (`playwright.config.ts`, `wheeldeal.spec.ts`). Screenshot evidence is captured across all major flows:

| FLOW | COVERAGE |
|---|---|
| Home | Hero, full page, CTA hover, navbar states (logged in / out) |
| Auth | Login (empty → filled → error → result), Sign Up (full flow) |
| Offers | Initial view, full page, card hover |
| Cars | All cars, card hover, detail |
| Booking | List, filters, card hover, detail, full booking flow (car → start date → end date) |
| Contact | Form fill, branch selector (all 4 branches), success message, map |
| About | Hero, mission, team, timeline, perks, footer CTA |
| Dashboard | Overview, stat cards, sidebar, all tabs (vehicles, rentals, clients, staff, branches, security, revenue, report builder) |
| Mobile | Home, about, contact, login, menu open |


## DESIGN PATTERNS

| PATTERN | IMPLEMENTATION |
|---|---|
| **Clean Architecture** | Four-layer separation: Domain → Application → Infrastructure → API |
| **Repository Pattern** | `IRepositories` interface in Domain, implementations in Infrastructure |
| **Unit of Work** | `UnitOfWork.cs` coordinates transactions across repositories |
| **Service Layer** | Business logic in `Application/Services/`, controllers remain thin |
| **DTO Mapping** | `MappingExtensions.cs` maps between domain entities and DTOs |
| **Global Exception Handling** | `ExceptionMiddleware.cs` catches and formats all unhandled exceptions |
| **DI via Extensions** | `ServiceExtensions.cs` registers all services cleanly in `Program.cs` |
| **Angular Standalone Components** | No NgModules — each component imports only what it needs |
| **HTTP Interceptors** | `auth.interceptor.ts` attaches JWT; `refresh.interceptor.ts` handles silent token renewal |
| **Route Guards** | `auth.guard.ts` protects client and admin routes |
| **Consistent Animation Vocabulary** | Two reusable button animation styles (`btn--reactive` lift, ghost ring) applied across all pages |
| **CSS Custom Properties Design System** | `var(--space-*)`, `var(--fs-*)`, `var(--color-primary)` used throughout SCSS |


## GETTING STARTED

### PREREQUISITES

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) + Angular CLI (`npm i -g @angular/cli`)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)

### BACKEND SETUP

```bash
cd car-rental-web-app.Server/API
```

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CarRentalDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Set your JWT secret:

```json
"Jwt": {
  "Key": "your-256-bit-secret-here",
  "Issuer": "CarRentalAPI",
  "Audience": "CarRentalClient"
}
```

Apply migrations and run:

```bash
dotnet ef database update
dotnet run
```

API available at `https://localhost:{port}` · Scalar docs at `/scalar`

### FRONTEND SETUP

```bash
cd car-rental-web-app.client
npm install
ng serve
```

Frontend available at `http://localhost:4200`

### RUNNING E2E TESTS

```bash
cd car-rental-web-app.client
npx playwright test
```

Playwright report available in `playwright-report/index.html`


## TEAM

| NAME | ROLE |
|---|---|
| Cojocaru Florin - Cristian | Full-Stack Developer — responsible for Frontend, Backend & Database |
| Nicoli Andrei - Claudiu | Full-Stack Developer — responsible for Frontend, Backend & Authentication |


## CONTRIBUTIONS

Project created by **Cojocaru Florin - Cristian** and **Nicoli Andrei - Claudiu** — **CSE.3**, University of Craiova / Faculty of Automatics, Computer Science and Electronics.

Contributions are welcome! If you have suggestions for improving the code or documentation, please submit a pull request.


## LICENSE

This project is licensed under the [MIT License](LICENSE).
