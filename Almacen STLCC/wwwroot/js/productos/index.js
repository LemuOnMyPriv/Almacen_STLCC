document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple para productos con filtro de fechas
    inicializarFiltroMultiple({
        'filtroCodigo': 0,          // Columna 0: Código
        'filtroNombre': 1,           // Columna 1: Nombre
        'filtroMarca': 2,            // Columna 2: Marca
        'filtroCategoria': 3,        // Columna 3: Categoría
        'filtroUnidadMedida': 4,     // Columna 4: Unidad de Medida
        'filtroProveedor': 5         // Columna 5: Proveedor(es)
    }, 'tablaProductos', {
        columnaFecha: 6,              // Columna 6: Inventario (no hay fecha visible, pero agregamos soporte)
        inputDesde: 'filtroFechaDesde',
        inputHasta: 'filtroFechaHasta'
    });

    inicializarPaginacion('tablaProductos', 10);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});