using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Almacen_STLCC.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly ILogger<MinioService> _logger;

        public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;

            _bucketName = configuration["MinIO:BucketName"]?.Trim() ?? "almacen";

            if (string.IsNullOrWhiteSpace(_bucketName) || _bucketName.Contains("/"))
                throw new InvalidOperationException($"Bucket inválido: '{_bucketName}'");
        }

        public async Task<string> SubirArchivo(IFormFile archivo, string carpeta = "actas")
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    throw new ArgumentException("Archivo inválido");

                if (string.IsNullOrWhiteSpace(carpeta))
                    carpeta = Path.GetExtension(archivo.FileName)
                        .TrimStart('.')
                        .ToLower();

                var bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(_bucketName));

                if (!bucketExists)
                {
                    _logger.LogInformation("Creando bucket {BucketName}", _bucketName);
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                }

                var extension = Path.GetExtension(archivo.FileName);
                var nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.FileName);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"{nombreSinExtension}_{timestamp}{extension}";
                var safeFileName = string.Concat(nombreArchivo.Split(Path.GetInvalidFileNameChars()));

                var rutaMinio = Path.Combine(carpeta, DateTime.Now.Year.ToString(), safeFileName)
                    .Replace("\\", "/");

                _logger.LogInformation("Subiendo archivo a: {Bucket}/{Ruta}", _bucketName, rutaMinio);

                using (var stream = archivo.OpenReadStream())
                {
                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(rutaMinio)
                        .WithStreamData(stream)
                        .WithObjectSize(archivo.Length)
                        .WithContentType(archivo.ContentType));
                }

                _logger.LogInformation("Archivo subido exitosamente: {Ruta}", rutaMinio);
                return rutaMinio;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "Error de MinIO al subir archivo: {Mensaje}", ex.Message);
                throw new InvalidOperationException($"Error al subir archivo a MinIO: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al subir archivo");
                throw new InvalidOperationException($"Error al procesar el archivo: {ex.Message}", ex);
            }
        }


        public async Task<Stream> DescargarArchivo(string rutaMinio)
        {
            try
            {

                _logger.LogInformation("Descargando archivo: {Ruta}", rutaMinio);

                var stream = new MemoryStream();

                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(rutaMinio)
                    .WithCallbackStream(s => s.CopyTo(stream)));

                stream.Position = 0;
                _logger.LogInformation("Archivo descargado exitosamente: {Ruta}", rutaMinio);
                return stream;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "Error de MinIO al descargar archivo: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Archivo no encontrado o error de MinIO: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al descargar archivo: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Error al descargar el archivo: {ex.Message}", ex);
            }
        }

        public async Task<string> ObtenerUrlTemporal(string rutaMinio, int minutosExpiracion = 15)
        {
            try
            {
                _logger.LogInformation("Generando URL temporal para: {Ruta}", rutaMinio);

                var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(rutaMinio)
                    .WithExpiry(minutosExpiracion * 60));

                return url;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "Error de MinIO al generar URL temporal: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Error al generar URL temporal: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar URL temporal: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Error al generar URL: {ex.Message}", ex);
            }
        }

        public async Task EliminarArchivo(string rutaMinio)
        {
            try
            {
                _logger.LogInformation("Eliminando archivo: {Ruta}", rutaMinio);

                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(rutaMinio));

                _logger.LogInformation("Archivo eliminado exitosamente: {Ruta}", rutaMinio);
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "Error de MinIO al eliminar archivo: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Error al eliminar archivo de MinIO: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar archivo: {Ruta}", rutaMinio);
                throw new InvalidOperationException($"Error al eliminar el archivo: {ex.Message}", ex);
            }
        }

        public bool ValidarArchivo(IFormFile archivo, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (archivo == null || archivo.Length == 0)
            {
                mensajeError = "No se ha seleccionado ningún archivo";
                return false;
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(archivo.FileName).ToLower();

            if (!extensionesPermitidas.Contains(extension))
            {
                mensajeError = "Solo se permiten archivos: JPG, PNG, PDF, DOC y DOCX";
                return false;
            }

            if (archivo.Length > 10 * 1024 * 1024)
            {
                mensajeError = "El archivo no debe superar los 10 MB";
                return false;
            }

            return true;
        }
    }
}
