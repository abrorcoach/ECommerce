# E-Commerce Backend - Clean Architecture Guide

## Architecture Overview

This project follows **Clean Architecture** principles with clear separation of concerns across four layers:

```
┌─────────────────────────────────────────────────────────┐
│                     API Layer                           │
│  - Controllers                                          │
│  - Program.cs (Dependency Injection)                    │
│  - Middleware & Configuration                           │
└──────────────────┬──────────────────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────────────────┐
│              Application Layer                          │
│  - Service Interfaces (IProductService, etc.)           │
│  - DTOs (Data Transfer Objects)                         │
│  - Service Implementations                              │
│  - Business Logic                                       │
└──────────────────┬──────────────────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────────────────┐
│            Infrastructure Layer                         │
│  - DbContext (Entity Framework)                         │
│  - Database Configurations                              │
│  - External Services (JwtService)                       │
└──────────────────┬──────────────────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────────────────┐
│                Domain Layer                             │
│  - Entities (User, Product, Order, etc.)                │
│  - Enums (UserRole, OrderStatus)                        │
│  - Domain Logic                                         │
│  - No Dependencies                                      │
└─────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (ECommerce.Domain)
**Purpose**: Core business entities and domain logic

**Contains**:
- Entities: `User`, `Category`, `Product`, `Order`, `OrderItem`
- Enums: `UserRole`, `OrderStatus`
- Domain models with no external dependencies

**Rules**:
- No dependencies on other layers
- Pure domain logic only
- Framework-agnostic

### 2. Application Layer (ECommerce.Application)
**Purpose**: Business logic and use cases

**Contains**:
- Service Interfaces: `IAuthService`, `IProductService`, etc.
- DTOs for data transfer
- Service Implementations with business rules
- Application-specific logic

**Dependencies**:
- Domain layer only
- Defines interfaces that Infrastructure implements

### 3. Infrastructure Layer (ECommerce.Infrastructure)
**Purpose**: External concerns and data access

**Contains**:
- `ApplicationDbContext` (EF Core)
- Database configurations
- External service implementations (`JwtService`)
- Data persistence logic

**Dependencies**:
- Domain layer
- Application layer
- External frameworks (EF Core, SQL Server)

### 4. API Layer (ECommerce.API)
**Purpose**: HTTP endpoints and request handling

**Contains**:
- Controllers: `AuthController`, `ProductsController`, etc.
- Dependency injection setup
- Middleware configuration
- API-specific concerns

**Dependencies**:
- All other layers
- ASP.NET Core framework

## Key Design Patterns

### 1. Dependency Inversion Principle
- High-level modules (Application) don't depend on low-level modules (Infrastructure)
- Both depend on abstractions (interfaces)

Example:
```csharp
// Application layer defines interface
public interface IProductService
{
    Task<ProductDto> GetProductById(int id);
}

// Infrastructure/Application implements
public class ProductService : IProductService
{
    // Implementation
}

// API layer uses abstraction
public class ProductsController
{
    private readonly IProductService _productService;
}
```

### 2. Repository Pattern (via EF Core)
- `ApplicationDbContext` acts as repository
- Services interact with DbContext for data access

### 3. Service Layer Pattern
- Business logic encapsulated in service classes
- Controllers are thin, delegating to services

## Data Flow

### Request Flow (e.g., Get Product)
```
1. HTTP Request
   ↓
2. ProductsController.GetProductById(id)
   ↓
3. IProductService.GetProductById(id)
   ↓
4. ProductService queries DbContext
   ↓
5. DbContext retrieves Product entity
   ↓
6. ProductService maps to ProductDto
   ↓
7. Controller returns HTTP Response
```

### Authentication Flow
```
1. POST /api/auth/login
   ↓
2. AuthController.Login(request)
   ↓
3. IAuthService.Login(request)
   ↓
4. AuthService validates credentials (BCrypt)
   ↓
5. IJwtService.GenerateToken(user)
   ↓
6. Return AuthResponse with JWT token
   ↓
7. Client stores token in localStorage
   ↓
8. Subsequent requests include token in Authorization header
```

## Entity Relationships

```
User (1) ──────< (N) Order (1) ──────< (N) OrderItem
                                             │
Category (1) ──< (N) Product (1) ────────────┘
```

### Relationship Details

- **User → Orders**: One-to-Many
  - A user can have multiple orders
  - Configured with `DeleteBehavior.Restrict`

- **Category → Products**: One-to-Many
  - A category can have multiple products
  - Configured with `DeleteBehavior.Restrict`

- **Order → OrderItems**: One-to-Many
  - An order contains multiple items
  - Configured with `DeleteBehavior.Cascade`

- **Product → OrderItems**: One-to-Many
  - A product can appear in multiple orders
  - Configured with `DeleteBehavior.Restrict`

## Security Architecture

### JWT Authentication
```
1. User logs in with credentials
2. Server validates and generates JWT token
3. Token contains claims: UserId, Email, Role
4. Client sends token in Authorization header
5. Server validates token on each request
6. Extracts user info from claims
```

### Authorization Levels

| Endpoint | Anonymous | User | Admin |
|----------|-----------|------|-------|
| POST /api/auth/register | ✓ | ✓ | ✓ |
| POST /api/auth/login | ✓ | ✓ | ✓ |
| GET /api/products | ✓ | ✓ | ✓ |
| POST /api/products | ✗ | ✗ | ✓ |
| GET /api/orders/my-orders | ✗ | ✓ | ✓ |
| GET /api/orders | ✗ | ✗ | ✓ |
| POST /api/orders | ✗ | ✓ | ✓ |
| PATCH /api/orders/{id}/status | ✗ | ✗ | ✓ |

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Role INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
```

### Categories Table
```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL
);
```

### Products Table
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL,
    ImageUrl NVARCHAR(500),
    CategoryId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

### Orders Table
```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL,
    ShippingAddress NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### OrderItems Table
```sql
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
```

## Best Practices Implemented

1. **Separation of Concerns**: Each layer has a single responsibility
2. **Dependency Injection**: All dependencies injected via constructor
3. **DTOs**: Data transfer objects prevent over-posting and control API surface
4. **Async/Await**: All database operations are asynchronous
5. **Configuration**: Settings externalized in appsettings.json
6. **Error Handling**: Consistent error responses
7. **Password Security**: BCrypt hashing with salt
8. **API Documentation**: Swagger/OpenAPI integration
9. **CORS**: Cross-origin resource sharing configured
10. **Code Organization**: Clean folder structure and naming conventions

## Extension Points

### Adding New Entity

1. Create entity in `ECommerce.Domain/Entities`
2. Add DbSet to `ApplicationDbContext`
3. Create DTOs in `ECommerce.Application/DTOs`
4. Define service interface in `ECommerce.Application/Interfaces`
5. Implement service in `ECommerce.Application/Services`
6. Create controller in `ECommerce.API/Controllers`
7. Register service in `Program.cs`
8. Create migration and update database

### Adding New Business Rule

1. Update service implementation in Application layer
2. Add validation in controller if needed
3. Update DTOs if data structure changes

### Adding Authentication Provider

1. Create new service implementing authentication interface
2. Register in `Program.cs`
3. Update authentication middleware configuration

## Configuration Guide

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "JwtSettings": {
    "SecretKey": "Your secure secret key (min 32 characters)",
    "Issuer": "Your API name",
    "Audience": "Your client app name"
  }
}
```

### Environment-Specific Settings

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides (create if needed)

## Testing Strategy

### Unit Tests (Recommended to add)
- Test service logic in isolation
- Mock DbContext using in-memory database
- Test DTOs and mappings

### Integration Tests (Recommended to add)
- Test API endpoints end-to-end
- Use WebApplicationFactory
- Test with real database (test container)

### Example Unit Test Structure
```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetProductById_ReturnsProduct_WhenExists()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var service = new ProductService(dbContext);

        // Act
        var result = await service.GetProductById(1);

        // Assert
        Assert.NotNull(result);
    }
}
```

## Performance Considerations

1. **Eager Loading**: Use `.Include()` to prevent N+1 queries
2. **Async Operations**: All I/O operations are async
3. **Indexing**: Email column indexed for fast user lookup
4. **Connection Pooling**: EF Core manages connection pool
5. **Caching**: Consider adding caching layer for frequently accessed data

## Deployment Checklist

- [ ] Update connection string for production database
- [ ] Change JWT secret key to production value
- [ ] Configure CORS for specific origins
- [ ] Enable HTTPS
- [ ] Set up logging (Application Insights, Serilog)
- [ ] Configure health checks
- [ ] Set up database migrations in CI/CD
- [ ] Add rate limiting
- [ ] Configure authentication options
- [ ] Review and test all authorization rules
