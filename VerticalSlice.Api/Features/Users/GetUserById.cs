using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using VerticalSlice.Api.Contracts;
using VerticalSlice.Api.Database;
using VerticalSlice.Api.Shared;

namespace VerticalSlice.Api.Features.Users
{
    public class GetUserById
    {
        public sealed class Query : IRequest<UserResponse>
        {
            public int Id { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotNull().WithMessage("Id is required.");
            }
        }

        internal sealed class QueryHandler : IRequestHandler<Query, UserResponse>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;
            public QueryHandler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<UserResponse> Handle(Query request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var user = await _dbContext.Users.Where(u => u.Id == request.Id).Select(request => new UserResponse
                {
                    Id = request.Id,
                    Name = request.Name,
                    Address = request.Address,
                    CreatedDate = request.CreatedDate,
                    UpdatedDate = request.UpdatedDate
                }).FirstOrDefaultAsync(cancellationToken);
                return user;
            }
        }
    }

    public class GetUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/user/getUserById/{id:int}", async (ISender sender, int id) =>
            {
                try
                {
                    var user = await sender.Send(new GetUserById.Query { Id = id });

                    if(user == null)
                    {
                        var response = new APIResponse
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            ErrorMessage = new List<string> { "User not found." }
                        };
                        
                    }

                    var successResponse = new APIResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Result = user
                    };

                    return Results.Ok(successResponse);
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
