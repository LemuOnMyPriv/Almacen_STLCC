document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro de tabla
    inicializarFiltroTabla('buscarCategoria', 'tablaCategorias', [0]);

    inicializarPaginacion('tablaCategorias', 10);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});