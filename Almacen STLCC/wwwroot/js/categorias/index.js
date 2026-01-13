document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro de tabla
    inicializarFiltroTabla('buscarCategoria', 'tablaCategorias', [0]);

    inicializarPaginacion('tablaCategorias', 25);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});