using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommBackend.CustomErrors.ExceptionFilterLayer
{
    public class GlobalExceptionLayer:IMiddleware
    {
        private readonly RequestDelegate next;
        public GlobalExceptionLayer(RequestDelegate _next)
        {
            next = _next;   
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            try {

                await next(context);
            }
            catch (Exception ex) {

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ex.InnerException.Message);
            }
        }
    }
}
