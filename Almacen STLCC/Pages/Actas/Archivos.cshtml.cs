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

        public Acta? Acta { get; set; } = null!;
        public List<Anexo> Anexos { get; set; } = new();

        [BindProperty]
        public int IdActa { get; set; }

        [BindProperty]
        public IFormFile Archivo { get; set; } = null!;

        public string ErrorMessage { get; set; } = string.Empty;

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
            if (!_minioService.ValidarArchivo(Archivo, out string mensajeError))
            {
                ErrorMessage = mensajeError;
                return await OnGetAsync(IdActa);
            }

            try
            {
                var rutaMinio = await _minioService.SubirArchivo(Archivo, "actas");

                var anexo = new Anexo
                {
                    Id_Acta = IdActa,
                    Nombre_Archivo = Archivo.FileName,
                    Tipo_Archivo = Path.GetExtension(Archivo.FileName).TrimStart('.').ToLower(),
                    Ruta_Minio = rutaMinio,
                    Bucket_Minio = "almacen",
                    Tamaño_Kb = (int)(Archivo.Length / 1024),
                    Fecha_Subida = DateTime.Now,
                    Acta = null!
                };

                _context.Anexos.Add(anexo);
                await _context.SaveChangesAsync();

                return RedirectToPage(new { id = IdActa });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al subir el archivo: {ex.Message}";
                return await OnGetAsync(IdActa);
            }
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
    }
}