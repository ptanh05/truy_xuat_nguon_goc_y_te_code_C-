using Microsoft.AspNetCore.Mvc;
using Nethereum.Signer;
using Nethereum.Util;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly Web3ContractService _web3ContractService;

        public AuthController(ILogger<AuthController> logger, Web3ContractService web3ContractService)
        {
            _logger = logger;
            _web3ContractService = web3ContractService;
        }

        [HttpPost("verify-signature")]
        public async Task<IActionResult> VerifySignature([FromBody] VerifySignatureRequest request)
        {
            try
            {
                // Verify signature
                var signer = new EthereumMessageSigner();
                var recoveredAddress = signer.EncodeUTF8AndEcRecover(request.Message, request.Signature);

                if (!recoveredAddress.Equals(request.Address, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { success = false, message = "Invalid signature" });
                }

                // Verify role on blockchain
                var userRole = await _web3ContractService.GetUserRoleAsync(request.Address);

                // Create session
                HttpContext.Session.SetString("WalletAddress", request.Address);
                HttpContext.Session.SetInt32("UserRole", userRole);

                _logger.LogInformation($"User authenticated: {request.Address}");

                return Ok(new
                {
                    success = true,
                    address = request.Address,
                    role = userRole,
                    message = "Authentication successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying signature: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true, message = "Logged out successfully" });
        }

        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            var walletAddress = HttpContext.Session.GetString("WalletAddress");
            var userRole = HttpContext.Session.GetInt32("UserRole");

            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            return Ok(new
            {
                success = true,
                walletAddress = walletAddress,
                userRole = userRole
            });
        }
    }

    public class VerifySignatureRequest
    {
        public string Message { get; set; }
        public string Signature { get; set; }
        public string Address { get; set; }
    }
}
