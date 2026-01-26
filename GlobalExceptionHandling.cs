using BookStoreManagmentSystem.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Build.Tasks;

namespace BookStoreManagmentSystem
{
    public class GlobalExceptionHandling : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var response = new Errors()
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorMessage = exception.Message,
                Title = "Something Went Wrong"
            };
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
