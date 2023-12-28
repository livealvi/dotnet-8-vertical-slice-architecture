using Carter;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading;
using VerticalSlice.Api.Contracts;
using VerticalSlice.Api.Database;
using VerticalSlice.Api.Entities;
using VerticalSlice.Api.Shared;

namespace VerticalSlice.Api.Features.Users
{
    public class GetAllUsers
    {
        public sealed class Query : IRequest<List<UserResponse>>{}

        internal sealed class QueryHandler : IRequestHandler<Query, List<UserResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            public QueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
            public async Task<List<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var users = await _dbContext.Users.ToListAsync(cancellationToken);
                return users.Adapt<List<UserResponse>>();
            }
        }
    }

    public class GetAllUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/user/getAllUsers", async (ISender sender, HttpContext context) =>
            {
                try
                {
                    var users = await sender.Send(new GetAllUsers.Query());

                    if (users == null)
                        return Results.NotFound("No users found.");

                    var successResponse = new APIResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Result = users
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
