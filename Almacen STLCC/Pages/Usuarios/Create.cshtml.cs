using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class CreateModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuario>();
        }

        [BindProperty]
        public RegisterUserViewModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                ErrorMessage = "No tienes permisos para crear usuarios";
                return Page();
            }

            if (string.IsNullOrEmpty(Input?.Contraseña) || Input.Contraseña.Length < 6)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
                return Page();
            }

            if (Input.Contraseña.Length > 20)
            {
                ErrorMessage = "La contraseña no puede exceder 20 caracteres";
                return Page();
            }

            if (Input.Contraseña != Input.ConfirmarContraseña)
            {
                ErrorMessage = "Las contraseñas no coinciden";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrige los errores en el formulario";
                return Page();
            }

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == Input.Usuario))
            {
                ErrorMessage = "El nombre de usuario ya existe";
                return Page();
            }

            var nuevoUsuario = new Usuario
            {
                NombreUsuario = Input.Usuario,
                Contraseña = _hasher.HashPassword(null, Input.Contraseña),
                Rol = "USUARIO"
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Usuario '{Input.Usuario}' creado exitosamente";
            ModelState.Clear();
            Input = null;

            return Page();
        }
    }

}