using Minio;
using Minio.DataModel.Args;

namespace Almacen_STLCC.Services
{
    public class MinioService(IMinioClient minioClient, IConfiguration configuration)
    {
        private readonly IMinioClient _minioClient = minioClient;
        private readonly string _bucketName = configuration["MinIO:BucketName"] ?? "almacen";

        public async Task<string> SubirArchivo(IFormFile archivo, string carpeta)
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucketName));
            }

            var extension = Path.GetExtension(archivo.FileName);
            var nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.FileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"{nombreSinExtension}_{timestamp}{extension}";
            var rutaMinio = $"{carpeta}/{DateTime.Now.Year}/{nombreArchivo}".Replace("//", "/");

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
                .WithCallbackStream((streamData) =>
                {
                    streamData.CopyTo(stream);
                }));

            stream.Position = 0;
            return stream;
        }

        public async Task<string> ObtenerUrlTemporal(string rutaMinio)
        {
            var url = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(rutaMinio)
                    .WithExpiry(60 * 15));

            return url;
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