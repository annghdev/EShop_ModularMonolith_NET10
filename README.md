# E-Shop Modular Monolith

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-green.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

The **E-Shop Modular Monolith** project is a comprehensive e-commerce system designed to sell all types of products with various variants and attributes. The project is built with modern **Modular Monolith** architecture, combined with **Domain Driven Design (DDD)** and **Event-Driven Architecture** to ensure scalability, maintainability, and readiness to upgrade necessary components to microservices.

## Project Goal & Current Status

> ⚠ **Important Note**  
> This project is primarily a **learning and architectural exploration project**, not a production-ready system.

### 🎯 Goal
- Explore **Modular Monolith architecture**
- Apply **Domain Driven Design (DDD)** in a real-world-like system
- Experiment with **Event-Driven architecture (CQRS, messaging, eventual consistency)**
- Serve as a **technical playground** for trying out patterns, not a finished product

### Current Status
- The project is **incomplete and under development (paused)**
- Only some modules are functional:
  - Catalog Management
  - Inventory Adjustment
- The following core features are **NOT fully implemented**:
  - ❌ Order processing
  - ❌ Payment integration
  - ❌ Discount and Promotion Engine
- Some flows are:
  - ⚠️ Mocked at frontend level
  - ⚠️ Contain known bugs or incomplete UI logic

### 💡 Notes
- There is **no more complete version** of this project beyond this repository
- Development is currently **paused due to personal priorities**
- The project may be continued in the future, but **no timeline is guaranteed**

### 🎥 Demo
A quick demo of the current state:
https://youtu.be/C_imIiYUH44


## Table of Contents

- [Key Features](#key-features)
- [Technologies Used](#technologies-used)
- [Architecture & Design Patterns](#architecture--design-patterns)
- [Module Roles](#module-roles)
- [Module ERDs](#module-erds)
- [Installation & Setup Guide](#installation--setup-guide)
- [Demo](#demo)
- [Sample Images](#sample-images)

## Key Features

- **Product Management**: Support for products with multiple variants and attributes
- **User Management**: Authentication and authorization system
- **Shopping Cart and Payment**: Complete purchasing process
- **Order Management**: Order tracking and processing
- **Advanced Search**: Using Elasticsearch to enhance user search experience
- **RESTful API**: Designed according to REST standards
- **Event-Driven**: Data synchronization and cross-module with RabbitMQ

## Technologies Used

### Core Technologies

- **SDK**: .NET 10.0
- **Framework**: ASP.NET Core Web API, Aspire
- **Database**: PostgreSQL with Entity Framework Core
- **Message Broker**: RabbitMQ with MassTransit
- **Search Engine**: Elasticsearch
- **Container**: Docker

### Libraries & Tools

| Library | Purpose | Version |
|---------|---------|---------|
| **Entity Framework Core** | ORM and Database Access | 10.0+ |
| **MediatR** | CQRS and Mediator Pattern | 12.5.0 |
| **FluentValidation** | Validation Rules | 12.1.1 |
| **AutoMapper** | Mapping | 14.0.0 |
| **Wolverine** | Message Broker | 5.13.0 |
| **Serilog** | Structured Logging | 3.0+ |
| **Swagger/OpenAPI** | API Documentation | 10.0+ |
| **xUnit** | Unit Testing | 2.9.3 |
| **Moq** | Unit Testing | 4.20.72 |
| **FluentAssertions** | Unit Testing | 7.2.0 |

## Architecture & Design Patterns

### 1. Modular Monolith Architecture

```
📁 src/
├── API/                    # API Gateway (ASP.NET Core)
├── Apps/                   # UI Webs/Apps
│   ├── BlazorAdmin         # Admin panel
│   └── react-app/			# Website
├── Aspire/                 # Infrastructure resource environment setup, application orchestration and centralized management
├── Auth/					# Authentication/Authorization 
├── Modules/                # Business modules
│   ├── Catalog/            # Product management
│   ├── Inventory/          # Inventory management
│   ├── Orders/				# Orders processing
│   ├── Payment/			# Payment processing
│   ├── Pricing/            # Price, coupon and promotion management
│   ├── Report/				# Statistics and reports
│   ├── Shipping/			# Shipping management
│   ├── ShoppingCart/		# Shopping cart management
│   └── Users/				# Users management
├── Shared/					# Shared between modules
│   ├── Contracts/          # Public DTOs and Events
│   ├── Infrastructure/     # Common infrastructure (EF Core, Messaging, Caching ...)
│   └── Kernel/             # Core abstractions, framework library
📁 Tests/				    # Unit & Integration Tests
```

**Benefits:**
- Save RAM and CPU, simplify deployment when no scaling needs yet
- Code sharing, easy maintenance.
- Separate responsibilities in the spirit of distributed systems, easily convertible to microservices when scaling is needed.

### 2. Clean Architecture

Each module follows **Clean Architecture** principles:

```
📁 Module/
├── Domain/                 # Business Logic & Entities
├── Application/            # Commands/Queries applying Vertical Slice and Event handlers
└── Infrastructure/         # External Concerns (DB, APIs, ...)
```

### 3. Domain Driven Design (DDD)

- **Rich Domain Entities**: Entities containing business logic
- **Value Objects**: Immutable objects representing concepts
- **Aggregates**: Groups of entities with consistency boundaries
- **Domain Events**: Business events for decoupling
- **Repositories**: Abstract data access

### 4. Event-Driven Architecture

- **CQRS Pattern**: Command Query Responsibility Segregation
- **Domain Events**: Loose coupling between modules
- **Integration Events**: Cross-module communication
- **Eventual Consistency**: Asynchronous processing

## Module Roles

### Core Modules

| Module | Responsibility |
|--------|----------------|
| **Catalog** | Product Management, Categories & Brands |
| **Inventory** | Stock Management |
| **Orders** | Order Process |
| **Payment** | Payment transaction |
| **Shipping** | Delivery |
| **ShoppingCart** | Cart Management |
| **Pricing** | Price change log, Promotion Campaigns, Coupon |
| **Report** | Revenue report, Analysis |

### Shared Components

| Component | Purpose |
|-----------|---------|
| **Contracts** | DTOs, Integration Events, Command and Queries |
| **Kernel** | Base classes, abstractions, extensions, utilities |
| **Infrastructure** | Caching, File Storage, Notification, EventBus, EF Base classes|

## Module ERDs

Entity-Relationship diagrams for each module (Catalog, Inventory, Orders, Payment, Pricing, Shipping, ShoppingCart, Users) are documented in a separate file:

**[→ Module ERDs (documents/erd.md)](documents/erd.md)**

Diagrams reflect each module's domain (aggregates and entities). References to IDs from other modules (e.g. `ProductId`, `OrderId`) are logical; each module uses its own database.

## Installation & Setup Guide

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Visual Studio 2022+**
- **Aspire 13+**
- **DockerDesktop** (for running dependent services like PostgreSQL, RabbitMQ, Elasticsearch)

### 1. Clone Repository

```bash
git clone https://github.com/annghdev/EShop_ModularMonolith_NET10.git
cd EShop_ModularMonolith_NET10
```

### 2. Open Solution

Open the **EShop_ModularMonolith.slnx** file

### 3. Configure Environment Variables

Create `appsettings.Development.json` file in the `src/API/` directory, override the ApiKey and secrets

### 4. Run Application (Backend)

- Ensure Docker Desktop is running
- Select Aspire.AppHost as the startup project and Run

### 5. Access Dashboards
- **Aspire Dashboard**: [http://localhost:17170](http://localhost:17170)
- **Backend API**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Blazor Admin**: [http://localhost:5033](http://localhost:5033)

### 6. Access Blazor Admin
- Login: admin - 123123
- Seed Product data: go settings -> Seed Sample Data (maybe you will see timeout debug error because ElasticSearch sync data but it's ok)
- Inventory Adjustment: Go "Tồn Kho" (Inventory) -> "Bảng lưu trữ" (Stocks) -> choose an item -> click "Điều chỉnh" (Adjust)

### 7. Run Frontend React

```bash
cd src/Apps/react-app
npm install
npm run dev
```

### ⚠️ Notes

- This project uses .NET Aspire (preview) features. If your local environment is not updated to Aspire version 13 (or compatible version), you may encounter issues when running the solution.
- Please make sure you have the correct .NET SDK and Aspire tooling installed before running the project.
- If you run into any startup or orchestration errors, double-check your Aspire version first.

## Demo

- **Web Demo**: [https://annghdev.online](https://annghdev.online)
- **Video Demo**: [YouTube](https://youtu.be/C_imIiYUH44)

## Sample Images

### Aspire Dashboard
| Overview | Resource Table |
| :---: | :---: |
| ![Aspire Dashboard](assets/aspire-dashboard.png) | ![Aspire Dashboard Table](assets/aspire-dashboard-table.png) |

### Frontend React
| Home Page | Product Listing |
| :---: | :---: |
| <img src="assets/frontend/home.png" width="400" /> | <img src="assets/frontend/products.png" width="400" /> |

| Product Details | Shopping Cart |
| :---: | :---: |
| <img src="assets/frontend/product-details.png" width="400" /> | <img src="assets/frontend/cart.png" width="400" /> |

| Checkout Order | Order Details | Order History |
| :---: | :---: | :---: |
| <img src="assets/frontend/order.png" /> | <img src="assets/frontend/order_details.png" /> | <img src="assets/frontend/history.png" /> |

### Blazor Admin Panel
| Product Table | Create Product (draft) |
| :---: | :---: |
| <img src="assets/backend/catalog-product-table.png" /> | <img src="assets/backend/catalog-product-create-draft.png" /> |

| Category tree | Attribute Table | Product Details |
| :---: | :---: | :---: |
| <img src="assets/backend/catalog-category-table.png" /> | <img src="assets/backend/catalog-attribute-table.png" /> | <img src="assets/backend/catalog-product-details.png" /> |

| Inventory Item table | Warehouse table | Inventory Adjustment modal | Inventory History |
| :---: | :---: | :---: | :---: |
| <img src="assets/backend/inventory-items.png" /> | <img src="assets/backend/inventory-warehouses.png" /> | <img src="assets/backend/inventory-adjustment.png" /> | <img src="assets/backend/inventory-history.png" /> |


---

## Contributing

All contributions are very welcome! You can create Issues and PRs if you want.

## License

This project is distributed under the MIT license. See the [LICENSE](LICENSE) file for more details.

## Contact

- **Email**: annghdev@gmail.com

---

**Star this repo** if you find the project useful!

---

📖 **Read this in other languages:**
- [Vietnamese](README_VIETNAMESE.md)
