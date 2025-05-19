using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelASP.NET.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public string? Username { get; set; }

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("Username");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}
