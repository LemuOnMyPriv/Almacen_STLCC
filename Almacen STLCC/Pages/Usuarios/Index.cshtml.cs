using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;

namespace Almacen_STLCC.Pages.Usuarios
{
    public class IndexModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public required List<Usuario> Usuarios { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            Usuarios = await _context.Usuarios
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar usuarios";
                return RedirectToPage();
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado";
                return RedirectToPage();
            }

            if (usuario.NombreUsuario == currentUsername)
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propio usuario";
                return RedirectToPage();
            }

            if (usuario.Rol == "ADMINISTRADOR" && currentUsername != "soporte")
            {
                TempData["ErrorMessage"] = "Solo el administrador del sistema puede eliminar a otros administradores";
                return RedirectToPage();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuario '{usuario.NombreUsuario}' eliminado exitosamente";
            return RedirectToPage();
        }
    }
}