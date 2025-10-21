namespace PharmaDNA.Middleware
{
    public class Web3AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<Web3AuthenticationMiddleware> _logger;

        public Web3AuthenticationMiddleware(RequestDelegate next, ILogger<Web3AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var walletAddress = context.Session.GetString("WalletAddress");
            
            if (!string.IsNullOrEmpty(walletAddress))
            {
                context.Items["WalletAddress"] = walletAddress;
                context.Items["UserRole"] = context.Session.GetInt32("UserRole");
            }

            await _next(context);
        }
    }
}
