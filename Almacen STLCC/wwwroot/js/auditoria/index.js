document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple de tabla con fechas
    inicializarFiltroMultiple({
        'filtroUsuario': 1,       // Columna 1: Usuario
        'filtroAccion': 2,        // Columna 2: Acción
        'filtroTabla': 3,         // Columna 3: Tabla
        'filtroDescripcion': 4    // Columna 4: Descripción
    }, 'tablaAuditoria', {
        columnaFecha: 0,           // Columna 0: Fecha y Hora
        inputDesde: 'filtroFechaDesde',
        inputHasta: 'filtroFechaHasta'
    });
});

inicializarPaginacion('tablaAuditoria', 25);