using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class ChangePasswordModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher;

        public ChangePasswordModel(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuario>();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public string TargetUsername { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La nueva contraseña es requerida")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            [DataType(DataType.Password)]
            public string NuevaContraseña { get; set; }

            [Required(ErrorMessage = "Debes confirmar la nueva contraseña")]
            [DataType(DataType.Password)]
            [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmarContraseña { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("/Usuarios/Index");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            if (usuario == null)
            {
                return RedirectToPage("/Usuarios/Index");
            }

            TargetUsername = username;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string username)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                ErrorMessage = "No tienes permisos para realizar esta acción";
                TargetUsername = username;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrige los errores en el formulario";
                TargetUsername = username;
                return Page();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado.";
                TargetUsername = username;
                return Page();
            }

            usuario.Contraseña = _hasher.HashPassword(usuario, Input.NuevaContraseña);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Contraseña de '{username}' actualizada correctamente.";
            ModelState.Clear();
            Input = null;
            TargetUsername = username;

            return Page();
        }
    }
}