# E-Commerce Backend API

A full-featured e-commerce backend built with ASP.NET Core Web API following Clean Architecture principles.

## Project Structure

```
ECommerce/
├── ECommerce.Domain/          # Domain layer - Entities and Enums
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   └── Enums/
│       ├── UserRole.cs
│       └── OrderStatus.cs
│
├── ECommerce.Application/     # Application layer - Interfaces, DTOs, Services
│   ├── DTOs/
│   │   ├── AuthDTOs.cs
│   │   ├── ProductDTOs.cs
│   │   ├── CategoryDTOs.cs
│   │   └── OrderDTOs.cs
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IJwtService.cs
│   │   ├── IProductService.cs
│   │   ├── ICategoryService.cs
│   │   └── IOrderService.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── ProductService.cs
│       ├── CategoryService.cs
│       └── OrderService.cs
│
├── ECommerce.Infrastructure/  # Infrastructure layer - DbContext, Services
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   └── Services/
│       └── JwtService.cs
│
└── ECommerce.API/            # API layer - Controllers, Configuration
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── ProductsController.cs
    │   ├── CategoriesController.cs
    │   └── OrdersController.cs
    ├── Program.cs
    └── appsettings.json
```

## Features

- **Clean Architecture** - Separation of concerns with Domain, Application, Infrastructure, and API layers
- **Entity Framework Core** - ORM with SQL Server
- **JWT Authentication** - Secure token-based authentication
- **Role-based Authorization** - Admin and User roles
- **RESTful API** - Following REST principles
- **Swagger/OpenAPI** - Interactive API documentation
- **CORS Support** - Cross-Origin Resource Sharing enabled

## Entities

### User
- Id, Email, PasswordHash, FirstName, LastName, Role
- One-to-Many relationship with Orders

### Category
- Id, Name, Description
- One-to-Many relationship with Products

### Product
- Id, Name, Description, Price, Stock, ImageUrl, CategoryId
- Many-to-One relationship with Category
- One-to-Many relationship with OrderItems

### Order
- Id, UserId, TotalAmount, Status, ShippingAddress, CreatedAt, UpdatedAt
- Many-to-One relationship with User
- One-to-Many relationship with OrderItems

### OrderItem
- Id, OrderId, ProductId, Quantity, Price
- Many-to-One relationships with Order and Product

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{categoryId}` - Get products by category
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category (Admin only)
- `PUT /api/categories/{id}` - Update category (Admin only)
- `DELETE /api/categories/{id}` - Delete category (Admin only)

### Orders
- `GET /api/orders` - Get all orders (Admin only)
- `GET /api/orders/my-orders` - Get current user's orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PATCH /api/orders/{id}/status` - Update order status (Admin only)

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   cd C:\Users\User\Desktop\Projects\e-commerce-front
   ```

2. **Update database connection string**

   Open `ECommerce.API/appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Create database migration**
   ```bash
   cd ECommerce.API
   dotnet ef migrations add InitialCreate --project ../ECommerce.Infrastructure --startup-project .
   ```

5. **Update database**
   ```bash
   dotnet ef database update --project ../ECommerce.Infrastructure --startup-project .
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access Swagger UI**

   Open browser and navigate to: `https://localhost:5001/swagger`

## Creating Admin User

After running the application, you can register a user and then manually update their role in the database:

```sql
UPDATE Users SET Role = 1 WHERE Email = 'admin@example.com';
```

Or you can add seed data in `ApplicationDbContext.cs`:

```csharp
modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = 1,
        Email = "admin@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        FirstName = "Admin",
        LastName = "User",
        Role = UserRole.Admin
    }
);
```

## Sample API Usage

### Register User
```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

### Create Product (Admin)
```json
POST /api/products
Authorization: Bearer {token}
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50,
  "imageUrl": "https://example.com/laptop.jpg",
  "categoryId": 1
}
```

### Create Order
```json
POST /api/orders
Authorization: Bearer {token}
{
  "shippingAddress": "123 Main St, City, Country",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

### Update Order Status (Admin)
```json
PATCH /api/orders/1/status
Authorization: Bearer {token}
{
  "status": "Shipped"
}
```

## Order Status Values
- `Pending` - Order placed, awaiting processing
- `Processing` - Order being prepared
- `Shipped` - Order shipped to customer
- `Delivered` - Order delivered successfully
- `Cancelled` - Order cancelled

## Security

- Passwords are hashed using BCrypt
- JWT tokens expire after 24 hours
- Role-based authorization protects admin endpoints
- CORS is configured (update in production)

## Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- SQL Server
- JWT Authentication
- BCrypt.Net for password hashing
- Swagger/OpenAPI

## License

MIT License
