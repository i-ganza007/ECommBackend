namespace ECommBackend.CustomErrors.ExceptionFilterLayer
{
    public class OperationCancelledHandler:IMiddleware
    {
        private readonly RequestDelegate _next;

        public OperationCancelledHandler(RequestDelegate next)
        {
            _next=next;
        }


        public async Task InvokeAsync(HttpContext context,RequestDelegate Next) {

            try {

                await Next(context);
            }
            catch (OperationCanceledException e) {
                context.Response.StatusCode = 499;
                context.Response.ContentType = "application/json";
                return;
            }
        
        }
    }
}
