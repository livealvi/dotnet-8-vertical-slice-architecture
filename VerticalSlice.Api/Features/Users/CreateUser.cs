using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using System.Net;
using VerticalSlice.Api.Contracts;
using VerticalSlice.Api.Database;
using VerticalSlice.Api.Entities;
using VerticalSlice.Api.Shared;
using static VerticalSlice.Api.Features.Users.CreateUser;

namespace VerticalSlice.Api.Features.Users
{
    public class CreateUser
    {
        public sealed class Command : IRequest<CreateUserRequest>
        {
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
                RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required."); 
            }
        }

        internal sealed class CommandHandler : IRequestHandler<Command, CreateUserRequest>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            public CommandHandler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<CreateUserRequest> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var user = new User
                {
                    Name = request.Name,
                    Address = request.Address,
                    CreatedDate = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return user.Adapt<CreateUserRequest>();
            }
        }
    }

    public class CreateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/user/createUser", async (CreateUserRequest request, ISender sender) =>
            {
                try
                {
                    var command = request.Adapt<CreateUser.Command>();
                    var user = await sender.Send(command);

                    var response = new APIResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.Created,
                        Result = user
                    };

                    return Results.Created("User created.", response);
                }
                catch (Exception ex)
                {
                    var errorResponse = new APIResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ErrorMessage = new List<string> { ex.ToString() }
                    };

                    return Results.Problem(errorResponse.ErrorMessage.ToString());
                }
            });
        }
    }
}
