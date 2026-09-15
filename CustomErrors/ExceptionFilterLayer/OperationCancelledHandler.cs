namespace ECommBackend.CustomErrors.ExceptionFilterLayer
{
    public class OperationCancelledHandler:IMiddleware
    {
        public async Task InvokeAsync(HttpContext context,RequestDelegate next) {

            try {

                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) {

                // Only the client hanging up gets 499; a cancellation from anywhere else is a real fault.
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = 499;
                context.Response.ContentType = "application/json";
            }

        }
    }
}
