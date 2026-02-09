using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;
using FluentValidation;
using Inventory.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class CreateWarehouse
{
    public record Command(CreateWarehouseRequest Request) : ICommand<WarehouseDto>
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Code)
                .NotEmpty().WithMessage("Code is required")
                .MaximumLength(50).WithMessage("Code cannot be greater than 50 characters");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot be greater than 200 characters");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Command, WarehouseDto>
    {
        public async Task<WarehouseDto> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var existing = await uow.WarehouseRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existing != null)
            {
                throw new ConflictException($"Warehouse {request.Code}");
            }

            if (request.IsDefault)
            {
                var currentDefault = await uow.WarehouseRepository.GetDefaultWarehouseAsync(cancellationToken);
                if (currentDefault != null)
                {
                    currentDefault.RemoveDefault();
                    uow.WarehouseRepository.Update(currentDefault);
                }
            }

            var warehouse = Warehouse.Create(request.Code, request.Name, null, request.IsDefault);
            uow.Warehouses.Add(warehouse);
            await uow.CommitAsync(cancellationToken);

            return new WarehouseDto(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                warehouse.Address?.Street,
                warehouse.Address?.City,
                warehouse.Address?.District,
                warehouse.Address?.Country,
                warehouse.Address?.PostalCode,
                warehouse.IsActive,
                warehouse.IsDefault);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/admin/inventory/warehouses", async (
                CreateWarehouseRequest request,
                ISender sender) =>
            {
                var result = await sender.Send(new Command(request));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("CreateWarehouse")
            .RequireAuthorization();
        }
    }
}
