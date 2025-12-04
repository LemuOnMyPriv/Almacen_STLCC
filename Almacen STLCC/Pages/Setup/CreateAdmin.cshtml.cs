using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;

namespace Almacen_STLCC.Pages.Setup
{
    public class CreateAdminModel(ApplicationDbContext context, IWebHostEnvironment env) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        private readonly PasswordHasher<Usuario> _hasher = new();
        private readonly IWebHostEnvironment _env = env;

        //link para crear el usuario administrador https://almacen.stlcc.gob.hn/Setup/CreateAdmin

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_env.IsDevelopment())
                return Content("Esta página solo está disponible en modo desarrollo");
                
            if (await _context.Usuarios.AnyAsync(u => u.Rol == "ADMINISTRADOR"))
                return Content("Ya existe un usuario administrador en el sistema");

            var tempUser = new Usuario
            {
                NombreUsuario = "soporte",
                Contraseña = string.Empty,
                Rol = "ADMINISTRADOR"
            };

            var admin = new Usuario
            {
                NombreUsuario = "soporte",
                Contraseña = _hasher.HashPassword(tempUser, "@soporte2025"),
                Rol = "ADMINISTRADOR"
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            return Content("Usuario administrador creado exitosamente\n\nEl usuario y contraseña han sido restablecidos a los valores por defecto. Si usted está autorizado y no conoce las credenciales, póngase en contacto con el administrador del sistema.");
        }
    }
}