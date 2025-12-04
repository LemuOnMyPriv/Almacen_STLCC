using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Services;

namespace Almacen_STLCC.Pages.Actas
{
    public class ArchivosModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly MinioService _minioService;

        public ArchivosModel(ApplicationDbContext context, MinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        public Acta? Acta { get; set; }
        public List<Anexo> Anexos { get; set; } = new();

        [BindProperty]
        public int IdActa { get; set; }

        [BindProperty]
        public List<IFormFile> Archivos { get; set; } = new();

        [BindProperty]
        public int IdAnexo { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Acta = await _context.Actas
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (Acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            Anexos = await _context.Anexos
                .Where(a => a.Id_Acta == id)
                .OrderByDescending(a => a.Fecha_Subida)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Archivos == null || !Archivos.Any())
            {
                TempData["ErrorMessage"] = "No se seleccionaron archivos";
                return RedirectToPage(new { id = IdActa });
            }

            int archivosSubidos = 0;
            int archivosRechazados = 0;
            var errores = new List<string>();

            foreach (var archivo in Archivos)
            {
                if (!_minioService.ValidarArchivo(archivo, out string mensajeError))
                {
                    archivosRechazados++;
                    errores.Add($"{archivo.FileName}: {mensajeError}");
                    continue;
                }

                try
                {
                    var rutaMinio = await _minioService.SubirArchivo(archivo, "actas");

                    var anexo = new Anexo
                    {
                        Id_Acta = IdActa,
                        Nombre_Archivo = archivo.FileName,
                        Tipo_Archivo = Path.GetExtension(archivo.FileName).TrimStart('.').ToLower(),
                        Ruta_Minio = rutaMinio,
                        Bucket_Minio = "almacen",
                        Tamaño_Kb = (int)(archivo.Length / 1024),
                        Fecha_Subida = DateTime.Now,
                        Acta = null!
                    };

                    _context.Anexos.Add(anexo);
                    archivosSubidos++;
                }
                catch (Exception ex)
                {
                    archivosRechazados++;
                    errores.Add($"{archivo.FileName}: Error al subir - {ex.Message}");
                }
            }

            if (archivosSubidos > 0)
            {
                await _context.SaveChangesAsync();
            }

            if (archivosSubidos > 0 && archivosRechazados == 0)
            {
                TempData["SuccessMessage"] = $" {archivosSubidos} archivo(s) subido(s) exitosamente";
            }
            else if (archivosSubidos > 0 && archivosRechazados > 0)
            {
                TempData["WarningMessage"] = $" {archivosSubidos} archivo(s) subido(s), {archivosRechazados} rechazado(s). Errores: {string.Join("; ", errores)}";
            }
            else
            {
                TempData["ErrorMessage"] = $" No se pudo subir ningún archivo. Errores: {string.Join("; ", errores)}";
            }

            return RedirectToPage(new { id = IdActa });
        }

        public async Task<IActionResult> OnGetDescargarAsync(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null) return NotFound();

            try
            {
                var stream = await _minioService.DescargarArchivo(anexo.Ruta_Minio);

                var contentType = anexo.Tipo_Archivo switch
                {
                    "pdf" => "application/pdf",
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    "doc" => "application/msword",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                return File(stream, contentType, anexo.Nombre_Archivo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar: {ex.Message}";
                return RedirectToPage(new { id = anexo.Id_Acta });
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync()
        {
            var anexo = await _context.Anexos.FindAsync(IdAnexo);
            if (anexo == null)
            {
                TempData["ErrorMessage"] = "Archivo no encontrado";
                return RedirectToPage(new { id = IdActa });
            }

            try
            {
                await _minioService.EliminarArchivo(anexo.Ruta_Minio);

                _context.Anexos.Remove(anexo);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Archivo '{anexo.Nombre_Archivo}' eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            }

            return RedirectToPage(new { id = anexo.Id_Acta });
        }
    }
}