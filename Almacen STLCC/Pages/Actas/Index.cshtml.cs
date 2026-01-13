using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private const int ITEMS_POR_PAGINA = 20;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ActaConRequisiciones> Actas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroRequisicion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroProveedor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroNumeroActa { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroF01 { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroOrdenCompra { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroProducto { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        // Paginación
        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }

        public bool HayFiltrosActivos => !string.IsNullOrWhiteSpace(FiltroRequisicion) ||
                                          !string.IsNullOrWhiteSpace(FiltroProveedor) ||
                                          !string.IsNullOrWhiteSpace(FiltroNumeroActa) ||
                                          !string.IsNullOrWhiteSpace(FiltroF01) ||
                                          !string.IsNullOrWhiteSpace(FiltroOrdenCompra) ||
                                          !string.IsNullOrWhiteSpace(FiltroProducto) ||
                                          FechaDesde.HasValue ||
                                          FechaHasta.HasValue;

        public Dictionary<string, ResumenAgrupado> ResumenAgrupaciones { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.Actas
                .Include(a => a.Proveedor)
                .Include(a => a.Requisiciones)
                .Include(a => a.DetallesActa)
                    .ThenInclude(d => d.Producto)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroProducto))
            {
                query = query.Where(a => a.DetallesActa.Any(d =>
                    d.Producto.Nombre_Producto.Contains(FiltroProducto) ||
                    d.Producto.Codigo_Producto.ToString().Contains(FiltroProducto)));
            }

            if (!string.IsNullOrWhiteSpace(FiltroRequisicion))
            {
                query = query.Where(a => a.Requisiciones.Any(r =>
                    r.Requisicion.Contains(FiltroRequisicion)));
            }

            if (!string.IsNullOrWhiteSpace(FiltroProveedor))
            {
                query = query.Where(a =>
                    a.Proveedor.Nombre_Proveedor.Contains(FiltroProveedor));
            }

            if (!string.IsNullOrWhiteSpace(FiltroNumeroActa))
            {
                query = query.Where(a => a.Numero_Acta.Contains(FiltroNumeroActa));
            }

            if (!string.IsNullOrWhiteSpace(FiltroF01))
            {
                query = query.Where(a => a.F01.Contains(FiltroF01));
            }

            if (!string.IsNullOrWhiteSpace(FiltroOrdenCompra))
            {
                query = query.Where(a => a.Orden_Compra != null &&
                    a.Orden_Compra.Contains(FiltroOrdenCompra));
            }

            if (FechaDesde.HasValue)
            {
                query = query.Where(a => a.Fecha >= FechaDesde.Value);
            }

            if (FechaHasta.HasValue)
            {
                query = query.Where(a => a.Fecha <= FechaHasta.Value);
            }

            TotalResultados = await query.CountAsync();
            TotalPaginas = (int)Math.Ceiling(TotalResultados / (double)ITEMS_POR_PAGINA);

            if (PaginaActual < 1) PaginaActual = 1;
            if (PaginaActual > TotalPaginas && TotalPaginas > 0) PaginaActual = TotalPaginas;

            var actasDb = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((PaginaActual - 1) * ITEMS_POR_PAGINA)
                .Take(ITEMS_POR_PAGINA)
                .ToListAsync();

            Actas = actasDb.Select(a => new ActaConRequisiciones
            {
                Acta = a,
                Requisiciones = a.Requisiciones.OrderBy(r => r.Requisicion).ToList()
            }).ToList();

            if (HayFiltrosActivos)
            {
                var todasLasActas = await query.ToListAsync();
                GenerarResumenesAgrupados(todasLasActas);
            }
        }

        private void GenerarResumenesAgrupados(List<Acta> actas)
        {
            ResumenAgrupaciones.Clear();

            if (!string.IsNullOrWhiteSpace(FiltroProducto))
            {
                AgruparPorProducto(actas);
            }
            else if (!string.IsNullOrWhiteSpace(FiltroProveedor) ||
                     !string.IsNullOrWhiteSpace(FiltroRequisicion) ||
                     FechaDesde.HasValue || FechaHasta.HasValue)
            {
                AgruparPorProveedor(actas);
            }
        }

        private void AgruparPorProducto(List<Acta> actas)
        {
            foreach (var acta in actas)
            {
                var productosActa = acta.DetallesActa
                    .Where(d => string.IsNullOrWhiteSpace(FiltroProducto) ||
                               d.Producto.Nombre_Producto.Contains(FiltroProducto) ||
                               d.Producto.Codigo_Producto.ToString().Contains(FiltroProducto))
                    .ToList();

                foreach (var detalle in productosActa)
                {
                    var key = detalle.Producto.Nombre_Producto;

                    if (!ResumenAgrupaciones.ContainsKey(key))
                    {
                        ResumenAgrupaciones[key] = new ResumenAgrupado
                        {
                            NombreGrupo = key,
                            TipoGrupo = "Producto",
                            CantidadTotal = 0,
                            Desglose = new Dictionary<string, DetalleDesglose>()
                        };
                    }

                    ResumenAgrupaciones[key].CantidadTotal += detalle.Cantidad;

                    foreach (var req in acta.Requisiciones)
                    {
                        if (!ResumenAgrupaciones[key].Desglose.ContainsKey(req.Requisicion))
                        {
                            ResumenAgrupaciones[key].Desglose[req.Requisicion] = new DetalleDesglose
                            {
                                Etiqueta = req.Requisicion,
                                Cantidad = 0
                            };
                        }
                        ResumenAgrupaciones[key].Desglose[req.Requisicion].Cantidad += detalle.Cantidad;
                    }

                    if (!acta.Requisiciones.Any())
                    {
                        var provKey = acta.Proveedor.Nombre_Proveedor;
                        if (!ResumenAgrupaciones[key].Desglose.ContainsKey(provKey))
                        {
                            ResumenAgrupaciones[key].Desglose[provKey] = new DetalleDesglose
                            {
                                Etiqueta = provKey,
                                Cantidad = 0
                            };
                        }
                        ResumenAgrupaciones[key].Desglose[provKey].Cantidad += detalle.Cantidad;
                    }
                }
            }
        }

        private void AgruparPorProveedor(List<Acta> actas)
        {
            var agrupadoPorProveedor = actas
                .GroupBy(a => a.Proveedor.Nombre_Proveedor)
                .OrderByDescending(g => g.Count());

            foreach (var grupo in agrupadoPorProveedor)
            {
                var key = grupo.Key;
                var cantidadActas = grupo.Count();

                ResumenAgrupaciones[key] = new ResumenAgrupado
                {
                    NombreGrupo = key,
                    TipoGrupo = "Proveedor",
                    CantidadTotal = cantidadActas,
                    Desglose = new Dictionary<string, DetalleDesglose>()
                };

                var requisiciones = grupo
                    .SelectMany(a => a.Requisiciones)
                    .GroupBy(r => r.Requisicion)
                    .OrderByDescending(g => g.Count());

                foreach (var reqGrupo in requisiciones)
                {
                    ResumenAgrupaciones[key].Desglose[reqGrupo.Key] = new DetalleDesglose
                    {
                        Etiqueta = reqGrupo.Key,
                        Cantidad = reqGrupo.Count()
                    };
                }

                if (!ResumenAgrupaciones[key].Desglose.Any())
                {
                    foreach (var acta in grupo.Take(5))
                    {
                        var label = $"{acta.F01} - {acta.Fecha:dd/MM/yyyy}";
                        ResumenAgrupaciones[key].Desglose[label] = new DetalleDesglose
                        {
                            Etiqueta = label,
                            Cantidad = 1
                        };
                    }
                }
            }
        }

        public class ActaConRequisiciones
        {
            public required Acta Acta { get; set; }
            public List<ActaRequisicion> Requisiciones { get; set; } = new();
        }

        public class ResumenAgrupado
        {
            public required string NombreGrupo { get; set; }
            public required string TipoGrupo { get; set; }
            public int CantidadTotal { get; set; }
            public Dictionary<string, DetalleDesglose> Desglose { get; set; } = new();
        }

        public class DetalleDesglose
        {
            public required string Etiqueta { get; set; }
            public int Cantidad { get; set; }
        }
    }
}