using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Services;

namespace Almacen_STLCC.Pages
{
    public class LoginModel : PageModel
    {
        private readonly LdapAuthenticationService _ldapService;
        private readonly ApplicationDbContext _context;
        public LoginModel(LdapAuthenticationService ldapService, ApplicationDbContext context)
        {
            _ldapService = ldapService;
            _context = context;
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

        public async Task<IActionResult> OnPostAsync()
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

            var usuarioLocal = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NombreUsuario == Username);

            if (usuarioLocal == null && !isInAlmacenGroup)
            {
                ErrorMessage = "No tienes permisos para acceder al sistema de almacén";
                return Page();
            }

            HttpContext.Session.SetString("Username", Username);

            if (usuarioLocal != null)
            {
                HttpContext.Session.SetString("DisplayName", usuarioLocal.NombreUsuario);
                HttpContext.Session.SetString("Email", "");
                HttpContext.Session.SetString("Rol", usuarioLocal.Rol);
                HttpContext.Session.SetString("TipoUsuario", "LOCAL");
            }
            else
            {
                HttpContext.Session.SetString("DisplayName", Username);
                HttpContext.Session.SetString("Email", "");
                HttpContext.Session.SetString("Rol", "USUARIO");
                HttpContext.Session.SetString("TipoUsuario", "AD");
            }

            return RedirectToPage("/Index");
        }
    }
}