using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;

namespace Almacen_STLCC.Pages.Setup
{
    public class CreateAdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher;
        private readonly IWebHostEnvironment _env;

        public CreateAdminModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuario>();
            _env = env;
        }

        //link para crear el usuario administrador https://localhost:7142/Setup/CreateAdmin

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_env.IsDevelopment())
                return Content("Esta página solo está disponible en modo desarrollo");
                
            if (await _context.Usuarios.AnyAsync(u => u.Rol == "ADMINISTRADOR"))
                return Content("Ya existe un usuario administrador en el sistema");

            var admin = new Usuario
            {
                NombreUsuario = "soporte",
                Contraseña = _hasher.HashPassword(null, "@soporte2025"),
                Rol = "ADMINISTRADOR"
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            return Content("Usuario administrador creado exitosamente\n\nUsuario: soporte\nContraseña: @soporte2025");
        }
    }
}