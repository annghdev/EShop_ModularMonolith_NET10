# Implement Wolverine with RabbitMQ

Tài liệu hướng dẫn cấu hình và sử dụng Wolverine.FX làm Message Broker (thay thế MassTransit) với RabbitMQ và PostgreSQL Transactional Outbox.

## 1. Cài đặt Packages

Cài đặt các gói NuGet cần thiết vào `Shared/Infrastructure` project:

```xml
<PackageReference Include="WolverineFx" Version="5.13.0" />
<PackageReference Include="WolverineFx.RabbitMQ" Version="5.13.0" />
<PackageReference Include="WolverineFx.Postgresql" Version="5.13.0" />
<PackageReference Include="WolverineFx.EntityFrameworkCore" Version="5.13.0" />
```

> [!NOTE]
> Nếu gặp lỗi `NU1608` do xung đột phiên bản .NET/EF Core, hãy thêm `Directory.Build.props` vào root solution để suppress warning này.

## 2. Implement Publisher

Tạo `WolverineEventPublisher` implement `IIntegrationEventPublisher`:

```csharp
using Contracts;
using Kernel.Application;
using Wolverine;

namespace Infrastructure;

public class WolverineEventPublisher(IMessageBus messageBus) : IIntegrationEventPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IntegrationEvent
    {
        await messageBus.PublishAsync(@event);
    }
}
```

Đăng ký service trong `DependencyInjection.cs`:
```csharp
services.AddScoped<IIntegrationEventPublisher, WolverineEventPublisher>();
```

## 3. Cấu hình Host (Program.cs)

Cấu hình Wolverine trong `Program.cs` của API project:

```csharp
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;

// ...
var builder = WebApplication.CreateBuilder(args);

// QUAN TRỌNG: Thêm dòng này để Wolverine tự động tạo tables khi khởi động
builder.Host.UseResourceSetupOnStartup(); 

builder.Host.UseWolverine(opts =>
{
    // 1. Cấu hình RabbitMQ Transport
    opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))
        .AutoProvision(); // Tự động tạo Exchange/Queue

    // 2. Cấu hình Transactional Outbox với PostgreSQL
    // Auto-creation mặc định là CreateOrUpdate nhưng cần UseResourceSetupOnStartup để apply
    opts.PersistMessagesWithPostgresql(builder.Configuration.GetConnectionString("infrasdb")!);
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

    // 3. Auto-discovery Handlers từ các Module
    // Wolverine sẽ tự động scan các class có tên kết thúc bằng Consumer hoặc Handler
    opts.Discovery.IncludeAssembly(typeof(Catalog.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Inventory.DependencyInjection).Assembly);
    // ... thêm các module khác
});
```

## 4. Implement Consumer (Handler)

Wolverine sử dụng convention-based discovery. Không cần implement interface đặc biệt nào, chỉ cần:
1. Class name kết thúc bằng `Consumer` hoặc `Handler`.
2. Method `Handle` nhận message làm tham số đầu tiên.
3. Các dependencies được inject qua method parameters (Logic injection).

Ví dụ:

```csharp
// File: src/Modules/Inventory/Application/IntegrationHandlers/ProductPublishedEventConsumer.cs

public class ProductPublishedEventConsumer
{
    // Interface IInventoryUnitOfWork được inject tự động
    public async Task Handle(
        ProductPublishedIntegrationEvent @event,
        IInventoryUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        // Xử lý logic
        await uow.CommitAsync(cancellationToken);
    }
}
```

## 5. Các lưu ý quan trọng

- **Schema Management**: Để Wolverine tự động tạo bảng (`wolverine_incoming_envelopes`, v.v.), bắt buộc phải gọi `builder.Host.UseResourceSetupOnStartup()` trong `Program.cs`. Default behavior set là `AutoCreate.CreateOrUpdate`.
- **Discovery**: Wolverine tự động tìm consumers, không cần đăng ký thủ công từng handler như MassTransit.
- **Dependency Injection**: Ưu tiên inject vào method `Handle` thay vì Constructor.

## 6. Các tables của Wolverine

Khi chạy thành công, Wolverine sẽ tạo 8 tables trong schema `public` (hoặc schema bạn config) của database `infrasdb`:

1.  **wolverine_envelopes**: Bảng chính lưu trữ các message.
2.  **wolverine_incoming_envelopes**: (Inbox) Lưu các message nhận được để đảm bảo tính năng idempotent và xử lý outbox cho message nhận.
3.  **wolverine_outgoing_envelopes**: (Outbox) Lưu các message cần gửi đi. Message được lưu tại đây trong cùng transaction với db business, sau đó được background worker gửi đi.
4.  **wolverine_dead_letter_queues**: Lưu trữ các message bị lỗi sau khi retry hết số lần quy định.
5.  **wolverine_nodes**: Lưu thông tin các node đang chạy trong cluster (nếu chạy nhiều instances).
6.  **wolverine_node_assignments**: Quản lý việc phân chia công việc giữa các node.
7.  **wolverine_queued_envelopes**: (Có thể xuất hiện tùy version/config) Dùng cho local queue persistence.
8.  **wolverine_scheduled_envelopes**: Lưu trữ các message được lên lịch gửi hoặc xử lý trong tương lai.
