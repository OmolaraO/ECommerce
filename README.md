# E-Commerce

A full-stack online store: an ASP.NET Core Web API with JWT authentication and a React + TypeScript frontend.

Customers can browse and filter products, manage a cart, check out and track or cancel their orders. Admins can manage the product catalogue and update order statuses.

## Tech stack

**Backend**
- ASP.NET Core 8 Web API
- SQL Server, accessed with Dapper
- JWT bearer authentication with role-based authorization (Admin policy)
- BCrypt password hashing
- FluentValidation for request validation
- Swagger / OpenAPI

**Frontend**
- React 18 + TypeScript
- Vite
- React Router

## Project structure

```
├── E-Commerce API/          # Web API: controllers, auth, middleware, startup (contains the .sln)
├── ECommerce.Application/   # Business logic: services, repositories, models, validators, DB setup
├── ECommerce.Contracts/     # Request and response DTOs shared by the API
└── ECommerce.Web/           # React frontend
```

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or LocalDB is fine)
- [Node.js](https://nodejs.org/) 18+

### 1. Database

Create an empty database called `ECommerceDb`. The tables are created automatically when the API starts.

Update the connection string in `E-Commerce API/appsettings.json` to point to your SQL Server instance:

```json
"Database": {
  "ConnectionString": "Server=YOUR_SERVER;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 2. JWT signing key

The signing key is kept out of source control. Set one with .NET user-secrets, using any random string of at least 32 characters:

```bash
cd "E-Commerce API"
dotnet user-secrets set "Jwt:Key" "<your-long-random-secret>"
```

### 3. Run the API

```bash
cd "E-Commerce API"
dotnet run --launch-profile http
```

The API runs on `http://localhost:5065`. Swagger UI is at `http://localhost:5065/swagger`.

In Development, an admin account is seeded from `appsettings.Development.json`:

| Username | Password    |
|----------|-------------|
| `admin`  | `Admin123!` |

> These are local development credentials only. Change them, or remove the `SeedAdmin` section, for any other environment.

### 4. Run the frontend

```bash
cd ECommerce.Web
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite dev server proxies `/api` requests to the API, so no CORS setup is needed.

## API endpoints

| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | `/api/auth/register` | Public | Register a customer account |
| POST | `/api/auth/login` | Public | Log in and receive a JWT |
| GET | `/api/products` | Public | List products (filter by `name`, `category`, `minPrice`, `maxPrice`; paginate with `page`, `pageSize`) |
| GET | `/api/products/{id}` | Public | Get a product |
| POST | `/api/products` | Admin | Create a product |
| PUT | `/api/products/{id}` | Admin | Update a product |
| DELETE | `/api/products/{id}` | Admin | Delete a product |
| GET | `/api/cart` | Authenticated | Get the current user's cart |
| POST | `/api/cart/items` | Authenticated | Add an item to the cart |
| PUT | `/api/cart/items/{productId}` | Authenticated | Change an item's quantity |
| DELETE | `/api/cart/items/{productId}` | Authenticated | Remove an item |
| DELETE | `/api/cart` | Authenticated | Clear the cart |
| POST | `/api/orders` | Authenticated | Check out the cart as an order |
| GET | `/api/orders` | Authenticated | List the current user's orders |
| GET | `/api/orders/{id}` | Authenticated | Get one of the user's orders |
| POST | `/api/orders/{id}/cancel` | Authenticated | Cancel a pending order |
| GET | `/api/admin/orders` | Admin | List all orders |
| PUT | `/api/admin/orders/{id}/status` | Admin | Update an order's status |

**Product categories:** Electronics, Clothing, Food, Books, HomeAndGarden, Sports, Toys, Others

**Order statuses:** Pending, Completed, Cancelled

## Frontend pages

- **Shop** (`/`): product listing with search and filters
- **Product** (`/products/:id`): product details and add to cart
- **Login / Register**
- **Cart** (`/cart`) and **Orders** (`/orders`, `/orders/:id`): signed-in users only
- **Admin** (`/admin/products`, `/admin/orders`): manage the catalogue and order statuses
