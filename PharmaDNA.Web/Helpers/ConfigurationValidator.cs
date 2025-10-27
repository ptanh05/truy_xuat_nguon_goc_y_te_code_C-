namespace PharmaDNA.Web.Helpers
{
    public static class ConfigurationValidator
    {
        public static void ValidateRequiredEnvironmentVariables()
        {
            var requiredVars = new[]
            {
                "DATABASE_URL",
                "PINATA_JWT", 
                "PHARMADNA_RPC",
                "PHARMA_NFT_ADDRESS",
                "OWNER_PRIVATE_KEY"
            };

            var missingVars = new List<string>();

            foreach (var varName in requiredVars)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(varName)))
                {
                    missingVars.Add(varName);
                }
            }

            if (missingVars.Any())
            {
                throw new InvalidOperationException(
                    $"Missing required environment variables: {string.Join(", ", missingVars)}. " +
                    "Please check your .env file and ensure all required variables are set."
                );
            }
        }

        public static void ValidateDatabaseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DATABASE_URL is not configured");
            }

            if (!connectionString.StartsWith("postgresql://"))
            {
                throw new InvalidOperationException("DATABASE_URL must be a valid PostgreSQL connection string");
            }
        }

        public static void ValidateBlockchainConfiguration()
        {
            var rpcUrl = Environment.GetEnvironmentVariable("PHARMADNA_RPC");
            var contractAddress = Environment.GetEnvironmentVariable("PHARMA_NFT_ADDRESS");
            var privateKey = Environment.GetEnvironmentVariable("OWNER_PRIVATE_KEY");

            if (string.IsNullOrEmpty(rpcUrl))
            {
                throw new InvalidOperationException("PHARMADNA_RPC is not configured");
            }

            if (string.IsNullOrEmpty(contractAddress))
            {
                throw new InvalidOperationException("PHARMA_NFT_ADDRESS is not configured");
            }

            if (string.IsNullOrEmpty(privateKey))
            {
                throw new InvalidOperationException("OWNER_PRIVATE_KEY is not configured");
            }

            if (!contractAddress.StartsWith("0x") || contractAddress.Length != 42)
            {
                throw new InvalidOperationException("PHARMA_NFT_ADDRESS must be a valid Ethereum address");
            }

            if (!privateKey.StartsWith("0x") || privateKey.Length != 66)
            {
                throw new InvalidOperationException("OWNER_PRIVATE_KEY must be a valid private key");
            }
        }

        public static void ValidateIPFSConfiguration()
        {
            var pinataJwt = Environment.GetEnvironmentVariable("PINATA_JWT");
            var gatewayUrl = Environment.GetEnvironmentVariable("PINATA_GATEWAY");

            if (string.IsNullOrEmpty(pinataJwt))
            {
                throw new InvalidOperationException("PINATA_JWT is not configured");
            }

            if (!pinataJwt.StartsWith("Bearer "))
            {
                throw new InvalidOperationException("PINATA_JWT must start with 'Bearer '");
            }

            if (string.IsNullOrEmpty(gatewayUrl))
            {
                throw new InvalidOperationException("PINATA_GATEWAY is not configured");
            }

            if (!Uri.TryCreate(gatewayUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("PINATA_GATEWAY must be a valid URL");
            }
        }

        public static void ValidateEmailConfiguration()
        {
            var smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST");
            var smtpPort = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT");
            var smtpUser = Environment.GetEnvironmentVariable("EMAIL_SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASS");

            // Email is optional, but if any email config is provided, all should be provided
            if (!string.IsNullOrEmpty(smtpHost) || !string.IsNullOrEmpty(smtpUser))
            {
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || 
                    string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    throw new InvalidOperationException(
                        "If email is configured, all email variables must be set: " +
                        "EMAIL_SMTP_HOST, EMAIL_SMTP_PORT, EMAIL_SMTP_USER, EMAIL_SMTP_PASS"
                    );
                }

                if (!int.TryParse(smtpPort, out var port) || port <= 0 || port > 65535)
                {
                    throw new InvalidOperationException("EMAIL_SMTP_PORT must be a valid port number");
                }
            }
        }

        public static void ValidateAll()
        {
            ValidateRequiredEnvironmentVariables();
            ValidateDatabaseConnectionString(Environment.GetEnvironmentVariable("DATABASE_URL")!);
            ValidateBlockchainConfiguration();
            ValidateIPFSConfiguration();
            ValidateEmailConfiguration();
        }
    }
}
