using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaDNA.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
            // Check if already logged in
            var walletAddress = HttpContext.Session.GetString("WalletAddress");
            if (!string.IsNullOrEmpty(walletAddress))
            {
                RedirectToPage("/Dashboard");
            }
        }
    }
}
