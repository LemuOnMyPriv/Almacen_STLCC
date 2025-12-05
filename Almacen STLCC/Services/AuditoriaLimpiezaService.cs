using Almacen_STLCC.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Almacen_STLCC.Services
{
    public class AuditoriaLimpiezaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditoriaLimpiezaService> _logger;

        public AuditoriaLimpiezaService(IServiceProvider serviceProvider, ILogger<AuditoriaLimpiezaService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var ahora = DateTime.Now;
                    var proximaEjecucion = new DateTime(ahora.Year, ahora.Month, 1, 2, 0, 0).AddMonths(1);

                    if (ahora.Day == 1 && ahora.Hour >= 2)
                    {
                        proximaEjecucion = proximaEjecucion.AddMonths(1);
                    }

                    var tiempoEspera = proximaEjecucion - ahora;

                    _logger.LogInformation("Próxima limpieza automática de auditorías: {Fecha}", proximaEjecucion);

                    await Task.Delay(tiempoEspera, stoppingToken);

                    await LimpiarAuditorias();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de limpieza automática de auditorías");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }

        private async Task LimpiarAuditorias()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var auditorias = await context.Auditorias
                    .OrderBy(a => a.Fecha_Hora)
                    .ToListAsync();

                if (!auditorias.Any())
                {
                    _logger.LogInformation("No hay auditorías para limpiar");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("=================================================================");
                sb.AppendLine($"AUDITORÍA DEL SISTEMA - EXPORTACIÓN AUTOMÁTICA");
                sb.AppendLine($"FECHA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"TOTAL DE REGISTROS: {auditorias.Count}");
                sb.AppendLine("=================================================================");
                sb.AppendLine();

                foreach (var audit in auditorias)
                {
                    sb.AppendLine($"[{audit.Fecha_Hora:dd/MM/yyyy HH:mm:ss}] {audit.Accion} en {audit.Tabla}");
                    sb.AppendLine($"  Usuario: {audit.Usuario}");
                    sb.AppendLine($"  ID Registro: {audit.Id_Registro}");
                    sb.AppendLine($"  Descripción: {audit.Descripcion}");
                    sb.AppendLine($"  IP: {audit.Ip_Address ?? "N/A"}");
                    sb.AppendLine();
                }

                var nombreArchivo = $"auditoria_auto_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var rutaArchivo = Path.Combine("Logs", "Auditorias", nombreArchivo);

                Directory.CreateDirectory(Path.Combine("Logs", "Auditorias"));

                await System.IO.File.WriteAllTextAsync(rutaArchivo, sb.ToString(), Encoding.UTF8);

                context.Auditorias.RemoveRange(auditorias);
                await context.SaveChangesAsync();

                _logger.LogInformation("Limpieza automática completada. {Count} registros exportados a {Archivo}",
                    auditorias.Count, nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar limpieza automática de auditorías");
            }
        }
    }
}