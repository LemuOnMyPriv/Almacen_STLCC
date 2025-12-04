using Minio;
using Minio.DataModel.Args;

namespace Almacen_STLCC.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public MinioService(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;

            _bucketName = configuration["MinIO:BucketName"]?.Trim() ?? "almacen";

            if (string.IsNullOrWhiteSpace(_bucketName) || _bucketName.Contains("/"))
                throw new InvalidOperationException($"Bucket inválido: '{_bucketName}'");
        }

        public async Task<string> SubirArchivo(IFormFile archivo, string carpeta = "actas")
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
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));

            var extension = Path.GetExtension(archivo.FileName);
            var nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.FileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"{nombreSinExtension}_{timestamp}{extension}";
            var safeFileName = string.Concat(nombreArchivo.Split(Path.GetInvalidFileNameChars()));

            var rutaMinio = Path.Combine(carpeta, DateTime.Now.Year.ToString(), safeFileName)
                .Replace("\\", "/");

            Console.WriteLine($"[MinIO] Subiendo archivo a: {_bucketName}/{rutaMinio}");

            using (var stream = archivo.OpenReadStream())
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(rutaMinio)
                    .WithStreamData(stream)
                    .WithObjectSize(archivo.Length)
                    .WithContentType(archivo.ContentType));
            }

            return rutaMinio;
        }

        public async Task<Stream> DescargarArchivo(string rutaMinio)
        {
            var stream = new MemoryStream();

            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(rutaMinio)
                .WithCallbackStream(s => s.CopyTo(stream)));

            stream.Position = 0;
            return stream;
        }

        public async Task<string> ObtenerUrlTemporal(string rutaMinio, int minutosExpiracion = 15)
        {
            return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(rutaMinio)
                .WithExpiry(minutosExpiracion * 60));
        }

        public async Task EliminarArchivo(string rutaMinio)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(rutaMinio));
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
