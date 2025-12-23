document.addEventListener("DOMContentLoaded", function () {
    inicializarFiltroMultiple({
        'filtroCodigo': 0,
        'filtroNombre': 1,
        'filtroMarca': 2,
        'filtroCategoria': 3
    }, 'tablaProductos');

    mostrarMensajeExito();
});