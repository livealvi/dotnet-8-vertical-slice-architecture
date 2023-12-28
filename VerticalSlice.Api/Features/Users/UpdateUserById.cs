using Azure.Core;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Api.Contracts;
using VerticalSlice.Api.Database;
using VerticalSlice.Api.Entities;

namespace VerticalSlice.Api.Features.Users
{
    public class UpdateUserById
    {
        public sealed class Command : IRequest<UpdateUserRequest>
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public string Address { get; set; } = default!;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotNull().WithMessage("Id is required.");
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
                RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required."); 
            }
        }

        internal sealed class CommandHandler : IRequestHandler<Command, UpdateUserRequest>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            public CommandHandler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<UpdateUserRequest> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var user = await _dbContext.Users.Where(u => u.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
                user.Name = request.Name;
                user.Address = request.Address;
                user.UpdatedDate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return user.Adapt<UpdateUserRequest>();
            }
        }
    }
    public class UpdateUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/user/updateUserById", async (UpdateUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserById.Command>();
                var update = await sender.Send(command);
                return Results.Ok(update);
            });
        }
    }
}
