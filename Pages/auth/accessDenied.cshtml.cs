using Microsoft.AspNetCore.Mvc.RazorPages;

public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
        HttpContext.Response.StatusCode = 403;
    }
}