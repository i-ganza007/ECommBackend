namespace ECommBackend.CustomErrors.ExceptionFilterLayer
{
    public class GlobalExceptionLayer : IMiddleware
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionLayer> _logger;

        public GlobalExceptionLayer(IHostEnvironment environment, ILogger<GlobalExceptionLayer> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            try {

                await next(context);
            }
            catch (Exception ex) {

                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                // Headers are already on the wire at this point, so there is no response left to rewrite.
                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    // Stack traces stay out of non-development responses.
                    stackTrace = _environment.IsDevelopment() ? ex.StackTrace : null
                });
            }
        }
    }
}
