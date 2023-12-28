using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace VerticalSlice.Api.Common
{
    public class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService(typeof(ISender)) as ISender;
    }
}
