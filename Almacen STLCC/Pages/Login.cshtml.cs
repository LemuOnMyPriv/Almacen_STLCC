using Almacen_STLCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Almacen_STLCC.Pages
{
    public class LoginModel(LdapAuthenticationService ldapService) : PageModel
    {
        private readonly LdapAuthenticationService _ldapService = ldapService;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Por favor ingrese usuario y contraseña";
                return Page();
            }

            var resultado = _ldapService.ValidateUserDetailed(Username, Password);

            if (!resultado.IsValid)
            {
                ErrorMessage = resultado.ErrorMessage;
                return Page();
            }

            HttpContext.Session.SetString("Username", Username);
            HttpContext.Session.SetString("DisplayName", resultado.DisplayName ?? Username);
            HttpContext.Session.SetString("Rol", resultado.Rol ?? "USUARIO");

            return RedirectToPage("/Index");
        }
    }
}