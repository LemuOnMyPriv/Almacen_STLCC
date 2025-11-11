using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Almacen_STLCC.Services;

namespace Almacen_STLCC.Pages
{
    public class LoginModel : PageModel
    {
        private readonly LdapAuthenticationService _ldapService;

        public LoginModel(LdapAuthenticationService ldapService)
        {
            _ldapService = ldapService;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Se requiere Usuario y Contraseña";
                return Page();
            }

            bool isValidUser = _ldapService.ValidateUser(Username, Password);
            
            if (!isValidUser)
            {
                ErrorMessage = "Usuario o contraseña incorrectos";
                return Page();
            }

            bool isInAlmacenGroup = _ldapService.IsUserInAlmacenGroup(Username);
            
            if (!isInAlmacenGroup)
            {
                ErrorMessage = "No tienes permisos para acceder al sistema de almacén";
                return Page();
            }

            HttpContext.Session.SetString("Username", Username);

            return RedirectToPage("/Index");
        }
    }
}