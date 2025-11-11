using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using Almacen_STLCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class CreateModel : PageModel
    {
        private readonly LdapAuthenticationService _authService;

        public CreateModel(LdapAuthenticationService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterUserViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            bool creado = _authService.RegisterLocalUser(Input.Usuario, Input.Contraseña);

            if (!creado)
            {
                ModelState.AddModelError(string.Empty, "El usuario ya existe");
                return Page();
            }

            return RedirectToPage("/Usuarios/List");
        }
    }
}