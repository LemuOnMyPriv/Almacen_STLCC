using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class EditModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id_Usuario { get; set; }

            [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
            [StringLength(100)]
            public required string NombreUsuario { get; set; }
            public required string Rol { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return RedirectToPage("/Usuarios/Index");
            }

            if (usuario.Rol == "ADMINISTRADOR" &&
                usuario.NombreUsuario != currentUsername &&
                currentUsername != "soporte")
            {
                TempData["ErrorMessage"] = "Solo el administrador del sistema puede editar a otros administradores";
                return RedirectToPage("/Usuarios/Index");
            }

            Input = new InputModel
            {
                Id_Usuario = usuario.Id_Usuario,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                ErrorMessage = "No tienes permisos para editar usuarios";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en los campos";
                return Page();
            }

            var usuario = await _context.Usuarios.FindAsync(Input.Id_Usuario);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado";
                return Page();
            }

            if (usuario.Rol == "ADMINISTRADOR" &&
                usuario.NombreUsuario != currentUsername &&
                currentUsername != "soporte")
            {
                ErrorMessage = "Solo el administrador del sistema puede editar a otros administradores";
                return Page();
            }

            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == Input.NombreUsuario && u.Id_Usuario != Input.Id_Usuario);

            if (usuarioExistente != null)
            {
                ErrorMessage = "El nombre de usuario ya existe";
                return Page();
            }

            usuario.NombreUsuario = Input.NombreUsuario;

            if (!string.IsNullOrEmpty(Input.Rol))
            {
                usuario.Rol = Input.Rol;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuario '{Input.NombreUsuario}' actualizado exitosamente";
            return RedirectToPage("/Usuarios/Index");
        }
    }
}