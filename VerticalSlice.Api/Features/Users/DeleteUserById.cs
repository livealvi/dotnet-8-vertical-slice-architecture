using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Api.Database;

namespace VerticalSlice.Api.Features.Users
{
    public class DeleteUserById
    {
        public sealed class Command : IRequest<bool>
        {
            public int Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotNull().WithMessage("Id is required.");
            }
        }

        internal sealed class CommandHandler : IRequestHandler<Command, bool>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            public CommandHandler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var user = await _dbContext.Users.Where(u => u.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }

    public class DeleteUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/user/deleteUserByid/{id:int}", async (ISender sender, int id ) =>
            {
                var user = await sender.Send(new DeleteUserById.Command { Id = id });
                return Results.Ok(user);
            });
        }
    }
}
