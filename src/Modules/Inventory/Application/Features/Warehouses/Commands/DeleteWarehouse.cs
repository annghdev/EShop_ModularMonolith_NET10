using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class DeleteWarehouse
{
    public record Command(Guid WarehouseId) : ICommand
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
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var warehouse = await uow.WarehouseRepository.GetByIdAsync(command.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Warehouse", command.WarehouseId);

            uow.WarehouseRepository.Remove(warehouse);
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/admin/inventory/warehouses/{id:guid}", async (
                Guid id,
                ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.NoContent();
            })
            .WithTags("Admin Inventory")
            .WithName("DeleteWarehouse")
            .RequireAuthorization();
        }
    }
}
