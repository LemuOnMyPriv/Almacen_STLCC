using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class ChangePasswordModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public ChangePasswordModel(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string TargetUsername { get; set; }
        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
            [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
            [DataType(DataType.Password)]
            public required string NuevaContraseña { get; set; }

            [Required(ErrorMessage = "Debe confirmar la contraseña")]
            [DataType(DataType.Password)]
            [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden")]
            public required string ConfirmarContraseña { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            var currentRol = HttpContext.Session.GetString("Rol");

            if (currentRol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            var targetUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            if (targetUser == null)
            {
                return RedirectToPage("/Usuarios/Index");
            }

            if (targetUser.Rol == "ADMINISTRADOR" &&
                targetUser.NombreUsuario != currentUsername &&
                currentUsername != "soporte")
            {
                TempData["ErrorMessage"] = "Solo el administrador del sistema puede cambiar las contraseñas de otros administradores";
                return RedirectToPage("/Usuarios/Index");
            }

            TargetUsername = username;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string username)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            var currentRol = HttpContext.Session.GetString("Rol");

            if (currentRol != "ADMINISTRADOR")
            {
                ErrorMessage = "No tienes permisos para cambiar contraseñas";
                TargetUsername = username;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                TargetUsername = username;
                return Page();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado";
                TargetUsername = username;
                return Page();
            }

            if (usuario.Rol == "ADMINISTRADOR" &&
                usuario.NombreUsuario != currentUsername &&
                currentUsername != "soporte")
            {
                ErrorMessage = "Solo el administrador del sistema puede cambiar las contraseñas de otros administradores";
                TargetUsername = username;
                return Page();
            }

            usuario.Contraseña = _passwordHasher.HashPassword(usuario, Input.NuevaContraseña);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Contraseña de '{usuario.NombreUsuario}' actualizada exitosamente";
            return RedirectToPage("/Usuarios/Index");
        }
    }
}