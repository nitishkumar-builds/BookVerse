# BookVerse — Online Bookstore

BookVerse is an ASP.NET Core MVC online bookstore built to practice real-world MVC development with authentication, database-driven catalog management, shopping cart flow, order management, and deployment to Windows hosting.

## Live Demo

[http://bookverse.runasp.net/](http://bookverse.runasp.net/)

## Tech Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Repository Pattern + Unit of Work
- AutoMapper
- Bootstrap 5
- Stripe checkout integration
- MonsterASP hosting on IIS / Windows Server

## Features

- Public book catalog with product details
- User registration and login using ASP.NET Core Identity
- Role-based areas for customer and admin workflows
- Product, category, company, and user management
- Shopping cart and order placement flow
- Tiered pricing support
- Stripe checkout integration when payment keys are configured
- Newsletter subscription storage
- Responsive UI for desktop and mobile screens
- EF Core migrations for database setup

## Screenshots

Add screenshots to these paths when available:

- `docs/screenshots/home.png`
- `docs/screenshots/books.png`
- `docs/screenshots/book-details.png`
- `docs/screenshots/cart.png`

## Project Structure

```text
BookVerse/
├── Controllers/          MVC controllers for account, catalog, cart, orders, and admin flows
├── DataAccess/           EF Core DbContext and data access setup
├── Mappings/             AutoMapper profiles
├── Migrations/           EF Core database migrations
├── Models/               Domain models and view models
├── Repositories/         Repository Pattern and Unit of Work implementation
├── Utilities/            Constants and helper classes
├── Views/                Razor views
├── wwwroot/              Static assets: CSS, JavaScript, images, libraries
├── Program.cs            App startup, dependency injection, middleware, and seeding
└── BookVerse.csproj      Project dependencies and target framework
```

## Setup Instructions

1. Clone the repository.

   ```bash
   git clone https://github.com/nitishkumar-builds/BookVerse.git
   cd BookVerse
   ```

2. Restore packages.

   ```bash
   dotnet restore
   ```

3. Create your local configuration.

   Copy `appsettings.Example.json` values into your own `appsettings.json`, then update the SQL Server connection string and optional Stripe keys.

4. Apply database migrations.

   ```bash
   dotnet ef database update
   ```

5. Run the application.

   ```bash
   dotnet run
   ```

6. Open the local URL shown in the terminal.

## Database Notes

The project uses Entity Framework Core migrations. For a new environment, update the connection string first, then run:

```bash
dotnet ef database update
```

For production hosting, use a production SQL Server connection string and keep real credentials outside source control.

## Demo Credentials

Demo credentials are not published in this README for safety. If seeded users are used during development, change their passwords before using the app publicly.

## Deployment

BookVerse is hosted on MonsterASP using IIS / Windows hosting. The deployed build should not include local logs, publish archives, user files, or real secrets in source control.

## What I Built / Learned

- Built an ASP.NET Core MVC project using layered folders and clean separation of concerns
- Used Entity Framework Core with SQL Server and migrations
- Implemented Identity-based authentication and role-aware workflows
- Practiced Repository Pattern and Unit of Work for data access
- Added cart, orders, product management, and responsive Razor views
- Published a .NET application to a live Windows hosting environment

