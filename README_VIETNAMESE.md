# E-Shop Modular Monolith

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-green.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Hệ thống thương mại điện tử được xây dựng theo kiến trúc Modular Monolith với Domain Driven Design và Event-Driven Architecture.

## 📋 Mục lục

- [Tổng quan](#-tổng-quan)
- [Công nghệ áp dụng](#-công-nghệ-áp-dụng)
- [Mẫu kiến trúc và triết lý thiết kế](#-mẫu-kiến-trúc-và-triết-lý-thiết-kế)
- [Vai trò các module](#-vai-trò-các-module)
- [Hướng dẫn cài đặt và chạy dự án](#-hướng-dẫn-cài-đặt-và-chạy-dự-án)
- [Demo và tài liệu](#-demo-và-tài-liệu)
- [Hình ảnh mẫu](#-hình-ảnh-mẫu)

## 🌟 Tổng quan

Dự án **E-Shop Modular Monolith** là một hệ thống thương mại điện tử toàn diện được thiết kế để bán tất cả loại sản phẩm với các biến thể và thuộc tính đa dạng. Dự án được xây dựng theo kiến trúc **Modular Monolith** hiện đại, kết hợp với **Domain Driven Design (DDD)** và **Event-Driven Architecture** để đảm bảo tính mở rộng, bảo trì và phát triển dễ dàng.

### Tính năng chính

- 🛍️ **Quản lý sản phẩm**: Hỗ trợ sản phẩm với nhiều biến thể và thuộc tính
- 👤 **Quản lý người dùng**: Hệ thống xác thực và phân quyền
- 🛒 **Giỏ hàng và thanh toán**: Quy trình mua hàng hoàn chỉnh
- 📊 **Quản lý đơn hàng**: Theo dõi và xử lý đơn hàng
- 🔍 **Tìm kiếm nâng cao**: Sử dụng Elasticsearch
- 📱 **API RESTful**: Thiết kế theo chuẩn REST
- 🔄 **Tích hợp sự kiện**: Đồng bộ dữ liệu cross-module với RabbitMQ

## 🛠️ Công nghệ áp dụng

### Core Technologies

- **SDK**: .NET 10.0
- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server với Entity Framework Core
- **Message Broker**: RabbitMQ với MassTransit
- **Search Engine**: Elasticsearch
- **Container**: Docker

### Libraries & Tools

| Thư viện | Mục đích | Phiên bản |
|----------|----------|-----------|
| **Entity Framework Core** | ORM và Database Access | 8.0+ |
| **MediatR** | CQRS và Mediator Pattern | 12.0+ |
| **FluentValidation** | Validation Rules | 11.0+ |
| **AutoMapper** | Object Mapping | 12.0+ |
| **MassTransit** | Message Broker | 8.0+ |
| **Serilog** | Structured Logging | 3.0+ |
| **Swagger/OpenAPI** | API Documentation | Built-in |
| **xUnit** | Unit Testing | 2.4+ |

## 🏗️ Mẫu kiến trúc và triết lý thiết kế

### 1. Modular Monolith Architecture

```
📁 src/
├── 🏛️ API/                    # Host chính (ASP.NET Core)
├── 📚 Modules/                 # Các module nghiệp vụ
│   ├── 👤 Identity/           # Quản lý danh tính
│   ├── 🛍️ Catalog/            # Quản lý danh mục sản phẩm
│   ├── 🛒 Ordering/           # Quản lý đơn hàng
│   ├── 💳 Payment/            # Xử lý thanh toán
│   └── 📦 Shipping/           # Vận chuyển
├── 🔧 Shared/                  # Chia sẻ giữa các module
│   ├── 🌱 Kernel/             # Core abstractions
│   └── 📋 Contracts/          # DTOs và Events
└── 🧪 Tests/                   # Unit & Integration Tests
```

**Lợi ích:**
- ✅ Đơn giản hóa deployment
- ✅ Chia sẻ code dễ dàng
- ✅ Transaction consistency
- ✅ Performance tốt hơn Microservices

### 2. Clean Architecture

Mỗi module tuân theo nguyên tắc **Clean Architecture**:

```
📁 Module/
├── 🏛️ Domain/                 # Business Logic & Entities
├── 🚀 Application/            # Use Cases & Commands/Queries
├── 🔧 Infrastructure/         # External Concerns (DB, APIs)
└── 🎯 Presentation/           # Controllers & DTOs
```

### 3. Domain Driven Design (DDD)

- **Rich Domain Entities**: Entities chứa business logic
- **Value Objects**: Immutable objects biểu diễn concepts
- **Aggregates**: Nhóm entities với consistency boundaries
- **Domain Events**: Business events cho decoupling
- **Repositories**: Abstract data access

### 4. Event-Driven Architecture

- **CQRS Pattern**: Command Query Responsibility Segregation
- **Domain Events**: Loose coupling giữa modules
- **Integration Events**: Cross-module communication
- **Eventual Consistency**: Asynchronous processing

```
Write Side (EF Core) ──── Events ────▶ Read Side (Elasticsearch)
       │                                        │
       └─────────────── MassTransit ────────────┘
                       RabbitMQ
```

## 📦 Vai trò các module

### Core Modules

| Module | Trách nhiệm | Database | Dependencies |
|--------|-------------|----------|--------------|
| **🏛️ API** | REST API Gateway<br/>Authentication & Authorization<br/>Request Routing | - | All Modules |
| **👤 Identity** | User Management<br/>Authentication<br/>Role-based Access | IdentityDb | Kernel |
| **🛍️ Catalog** | Product Management<br/>Categories & Attributes<br/>Inventory | CatalogDb | Kernel |
| **🛒 Ordering** | Order Processing<br/>Cart Management<br/>Order History | OrderingDb | Catalog, Identity |
| **💳 Payment** | Payment Processing<br/>Payment Methods<br/>Transactions | PaymentDb | Ordering |
| **📦 Shipping** | Shipping Methods<br/>Tracking<br/>Delivery | ShippingDb | Ordering |

### Shared Components

| Component | Mục đích |
|-----------|----------|
| **🌱 Kernel** | Base classes, abstractions, utilities |
| **📋 Contracts** | DTOs, Integration Events, Queries |

## 🚀 Hướng dẫn cài đặt và chạy dự án

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc [Docker](https://www.docker.com/)
- [RabbitMQ](https://www.rabbitmq.com/) (khuyến nghị dùng Docker)
- [Elasticsearch](https://www.elastic.co/downloads/elasticsearch) (khuyến nghị dùng Docker)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/your-username/e-shop-modular-monolith.git
cd e-shop-modular-monolith
```

### 2. Cài đặt Dependencies

```bash
# Khôi phục NuGet packages
dotnet restore

# Hoặc sử dụng script (Windows)
.\build.ps1 restore
```

### 3. Cấu hình Database

#### Sử dụng SQL Server Local

```sql
-- Tạo databases
CREATE DATABASE EShop_Identity;
CREATE DATABASE EShop_Catalog;
CREATE DATABASE EShop_Ordering;
CREATE DATABASE EShop_Payment;
CREATE DATABASE EShop_Shipping;
```

#### Sử dụng Docker Compose

```bash
# Chạy infrastructure services
docker-compose -f docker-compose.infrastructure.yml up -d
```

### 4. Cấu hình Environment Variables

Tạo file `appsettings.Development.json` trong thư mục `src/API/`:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Server=localhost;Database=EShop_Identity;Trusted_Connection=True;",
    "CatalogConnection": "Server=localhost;Database=EShop_Catalog;Trusted_Connection=True;",
    "OrderingConnection": "Server=localhost;Database=EShop_Ordering;Trusted_Connection=True;",
    "PaymentConnection": "Server=localhost;Database=EShop_Payment;Trusted_Connection=True;",
    "ShippingConnection": "Server=localhost;Database=EShop_Shipping;Trusted_Connection=True;"
  },
  "MessageBroker": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Elasticsearch": {
    "Url": "http://localhost:9200"
  }
}
```

### 5. Chạy Database Migrations

```bash
# Chạy migrations cho tất cả modules
dotnet run --project src/API/EShop.API.csproj -- --migrate
```

### 6. Chạy ứng dụng

```bash
# Development mode
dotnet run --project src/API/EShop.API.csproj

# Hoặc sử dụng Visual Studio
# Mở EShop.API.sln và nhấn F5
```

### 7. Truy cập API

- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api

### 8. Chạy Tests

```bash
# Chạy tất cả tests
dotnet test

# Chạy với coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🎬 Demo và tài liệu

### Web Demo
- 🌐 **Live Demo**: [https://e-shop-demo.vercel.app](https://e-shop-demo.vercel.app)
- 📱 **Mobile Demo**: [https://e-shop-mobile-demo.vercel.app](https://e-shop-mobile-demo.vercel.app)

### Video Tutorials
- 📹 **Architecture Overview**: [YouTube](https://youtube.com/watch?v=...)
- 📹 **Installation Guide**: [YouTube](https://youtube.com/watch?v=...)
- 📹 **DDD Implementation**: [YouTube](https://youtube.com/watch?v=...)

### Documentation
- 📚 **API Documentation**: [Swagger/OpenAPI](https://localhost:5001/swagger)
- 📖 **Architecture Docs**: [docs/architecture.md](docs/architecture.md)
- 🔧 **Contributing Guide**: [CONTRIBUTING.md](CONTRIBUTING.md)

## 📸 Hình ảnh mẫu

### Architecture Diagram
![Modular Monolith Architecture](docs/images/architecture-diagram.png)

### Database Schema
![Database Schema](docs/images/database-schema.png)

### API Flow
![API Flow](docs/images/api-flow.png)

### Domain Model
![Domain Model](docs/images/domain-model.png)

---

## 🤝 Đóng góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [CONTRIBUTING.md](CONTRIBUTING.md) để biết thêm chi tiết.

## 📄 Giấy phép

Dự án này được phân phối dưới giấy phép MIT. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

## 📞 Liên hệ

- **Email**: contact@e-shop-project.com
- **GitHub Issues**: [Issues](https://github.com/your-username/e-shop-modular-monolith/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-username/e-shop-modular-monolith/discussions)

---

⭐ **Star this repo** nếu bạn thấy dự án hữu ích!
