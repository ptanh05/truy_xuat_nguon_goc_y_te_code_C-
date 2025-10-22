using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaDNA.Attributes;

namespace PharmaDNA.Pages
{
    [RequireAuthentication]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // Dashboard page - requires authentication
        }
    }
}
