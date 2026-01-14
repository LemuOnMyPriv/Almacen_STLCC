document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple de tabla
    inicializarFiltroMultiple({
        'filtroNombreProveedor': 0,  // Columna 0: Nombre
        'filtroRTN': 1                // Columna 1: RTN
    }, 'tablaProveedores');

    inicializarPaginacion('tablaProveedores', 10);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});