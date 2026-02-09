using Contracts.Requests.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class UpdateWarehouse
{
    public record Command(Guid WarehouseId, UpdateWarehouseRequest Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.WarehouseId)
                .NotEmpty().WithMessage("WarehouseId is required");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot be greater than 200 characters");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var warehouse = await uow.WarehouseRepository.GetByIdAsync(command.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Warehouse", command.WarehouseId);

            warehouse.Update(command.Request.Name, warehouse.Address);
            uow.WarehouseRepository.Update(warehouse);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/admin/inventory/warehouses/{id:guid}", async (
                Guid id,
                UpdateWarehouseRequest request,
                ISender sender) =>
            {
                await sender.Send(new Command(id, request));
                return Results.Accepted();
            })
            .WithTags("Admin Inventory")
            .WithName("UpdateWarehouse")
            .RequireAuthorization();
        }
    }
}
