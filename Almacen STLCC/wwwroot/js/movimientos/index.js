document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple para movimientos con filtro de fechas
    inicializarFiltroMultiple({
        'filtroProducto': 1,   // Columna 1: Producto
        'filtroTipo': 2,       // Columna 2: Tipo de Movimiento
        'filtroActa': 4        // Columna 4: Acta Asociada
    }, 'tablaMovimientos', {
        columnaFecha: 0,       // Columna 0: Fecha
        inputDesde: 'filtroFechaDesde',
        inputHasta: 'filtroFechaHasta'
    });

    // Filtro adicional para cantidad
    const filtroCantidad = document.getElementById('filtroCantidad');
    if (filtroCantidad) {
        filtroCantidad.addEventListener('input', filtrarPorCantidad);
    }

    inicializarPaginacion('tablaMovimientos', 10);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});

function filtrarPorCantidad() {
    const tabla = document.getElementById('tablaMovimientos');
    if (!tabla) return;

    const tbody = tabla.querySelector('tbody');
    const filtroCantidad = document.getElementById('filtroCantidad');
    const valorFiltro = filtroCantidad.value.trim();

    if (!valorFiltro) {
        // Si está vacío, no filtrar por cantidad
        return;
    }

    const filas = tbody.getElementsByTagName('tr');

    for (let i = 0; i < filas.length; i++) {
        const fila = filas[i];

        if (fila.classList.contains('mensaje-vacio') ||
            fila.classList.contains('fila-sin-resultados')) {
            continue;
        }

        // Columna 3 es la cantidad
        const celdaCantidad = fila.cells[3];
        if (celdaCantidad) {
            const textoCantidad = celdaCantidad.textContent.replace(/[+-]/g, '').trim();
            const cantidad = parseInt(textoCantidad);
            const filtro = parseInt(valorFiltro);

            if (cantidad === filtro) {
                fila.style.display = '';
            } else {
                fila.style.display = 'none';
            }
        }
    }
}