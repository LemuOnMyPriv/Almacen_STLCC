document.addEventListener("DOMContentLoaded", function () {
    inicializarFiltroMultiple({
        'filtroNumeroActa': 0,
        'filtroF01': 1,
        'filtroOrdenCompra': 2,
        'filtroProveedor': 3
    }, 'tablaActas');

    mostrarMensajeExito();
});