using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Almacen_STLCC.Pages
{
    public class SecurePageModel : PageModel
    {
        public string CurrentUsername => HttpContext.Session.GetString("Username") ?? "";
        public string CurrentDisplayName => HttpContext.Session.GetString("DisplayName") ?? "";

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                context.Result = RedirectToPage("/Login");
                return;
            }

            base.OnPageHandlerExecuting(context);
        }
    }
}