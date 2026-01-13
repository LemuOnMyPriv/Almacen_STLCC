using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Services
{
    public class AuditoriaService
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Registrar(
            string usuario, 
            string accion, 
            string tabla, 
            int idRegistro, 
            string? descripcion = null, 
            string? ipAddress = null)
        {
            try
            {
                var audit = new Auditoria
                {
                    Usuario = usuario,
                    Accion = accion,
                    Tabla = tabla,
                    Id_Registro = idRegistro,
                    Descripcion = descripcion,
                    Fecha_Hora = DateTime.Now,
                    Ip_Address = ipAddress
                };

                _context.Auditorias.Add(audit);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando auditoría: {ex.Message}");
            }
        }

        public List<Auditoria> ObtenerHistorial(string tabla, int idRegistro)
        {
            return _context.Auditorias
                .Where(a => a.Tabla == tabla && a.Id_Registro == idRegistro)
                .OrderByDescending(a => a.Fecha_Hora)
                .ToList();
        }

        public List<Auditoria> ObtenerPorUsuario(string usuario, int limite = 100)
        {
            return _context.Auditorias
                .Where(a => a.Usuario == usuario)
                .OrderByDescending(a => a.Fecha_Hora)
                .Take(limite)
                .ToList();
        }

        public List<Auditoria> ObtenerRecientes(int dias = 7, int limite = 100)
        {
            var fechaDesde = DateTime.Now.AddDays(-dias);
            
            return _context.Auditorias
                .Where(a => a.Fecha_Hora >= fechaDesde)
                .OrderByDescending(a => a.Fecha_Hora)
                .Take(limite)
                .ToList();
        }
    }
}