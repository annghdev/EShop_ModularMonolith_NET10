using Contracts;
using Contracts.Requests.Inventory;
using FluentValidation;
using Inventory.Domain;
using Kernel.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class ImportItemBySku
{
    public record Command(ImportItemBySkuRequest Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.WarehouseId)
                .NotEmpty().WithMessage("WarehouseId is required");

            RuleFor(c => c.Request.Sku)
                .NotEmpty().WithMessage("Sku is required")
                .MaximumLength(50).WithMessage("Sku cannot be greater than 50 characters");

            RuleFor(c => c.Request.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        }
    }

    public class Handler(IInventoryUnitOfWork uow, IIntegrationRequestSender requestSender)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var warehouse = await uow.WarehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Warehouse", request.WarehouseId);

            var item = await uow.InventoryItemRepository
                .GetBySkuAndWarehouseAsync(request.Sku, request.WarehouseId, cancellationToken);

            if (item is null)
            {
                var productInfo = await requestSender.SendQueryAsync<GetProductVariantBySkuIntegrationQuery, ProductVariantBySkuResponse>(
                    new GetProductVariantBySkuIntegrationQuery("Inventory", request.Sku),
                    cancellationToken);

                item = InventoryItem.Create(
                    warehouse.Id,
                    productInfo.ProductId,
                    productInfo.VariantId,
                    new Sku(productInfo.Sku),
                    productInfo.ProductName,
                    initialQuantity: 0,
                    lowStockThreshold: 5);

                item.Receive(request.Quantity, "Nhập từ trang quản trị");
                uow.InventoryItems.Add(item);
            }
            else
            {
                item.Receive(request.Quantity, "Nhập từ trang quản trị");
            }

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/admin/inventory/items/import", async (
                ImportItemBySkuRequest request,
                ISender sender) =>
            {
                await sender.Send(new Command(request));
                return Results.Accepted();
            })
            .WithTags("Admin Inventory")
            .WithName("ImportItemBySku")
            .RequireAuthorization();
        }
    }
}
