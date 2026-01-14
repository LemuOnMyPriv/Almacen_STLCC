document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple para actas con filtro de fechas
    inicializarFiltroMultiple({
        'filtroNumeroActa': 0,   // Columna 0: Numero de Acta
        'filtroF01': 1,           // Columna 1: F01
        'filtroOrdenCompra': 2,   // Columna 2: Orden de Compra
        'filtroProveedor': 3      // Columna 3: Proveedor
    }, 'tablaActas', {
        columnaFecha: 5,           // Columna 5: Fecha
        inputDesde: 'filtroFechaDesde',
        inputHasta: 'filtroFechaHasta'
    });

    // Inicializar sistema de agrupación
    inicializarAgrupacionActas();

    inicializarPaginacion('tablaActas', 10);

    // Mostrar mensaje de éxito si existe
    mostrarMensajeExito();
});

function inicializarAgrupacionActas() {
    const tabla = document.getElementById('tablaActas');
    if (!tabla) return;

    const tbody = tabla.querySelector('tbody');
    const resumenContainer = document.getElementById('resumenAgrupado');

    if (!tbody || !resumenContainer) return;

    // Función para verificar si hay filtros activos
    function hayFiltrosActivos() {
        const inputs = document.querySelectorAll('.panel-busqueda-avanzado input');
        return Array.from(inputs).some(input => input.value.trim() !== '');
    }

    // Función para actualizar el resumen
    function actualizarResumen() {
        // Solo mostrar si hay filtros activos
        if (!hayFiltrosActivos()) {
            resumenContainer.style.display = 'none';
            return;
        }

        const filas = Array.from(tbody.getElementsByTagName('tr')).filter(fila =>
            !fila.classList.contains('mensaje-vacio') &&
            !fila.classList.contains('fila-sin-resultados') &&
            fila.style.display !== 'none'
        );

        if (filas.length === 0) {
            resumenContainer.style.display = 'none';
            return;
        }

        // Agrupar por proveedor
        const porProveedor = {};
        filas.forEach(fila => {
            const proveedor = fila.cells[3].textContent.trim();
            porProveedor[proveedor] = (porProveedor[proveedor] || 0) + 1;
        });

        // Agrupar por F01
        const porF01 = {};
        filas.forEach(fila => {
            const f01 = fila.cells[1].textContent.trim();
            porF01[f01] = (porF01[f01] || 0) + 1;
        });

        // Agrupar por Orden de Compra
        const porOrdenCompra = {};
        filas.forEach(fila => {
            const orden = fila.cells[2].textContent.trim();
            if (orden && orden !== 'N/A') {
                porOrdenCompra[orden] = (porOrdenCompra[orden] || 0) + 1;
            }
        });

        // Agrupar por requisiciones
        const porRequisicion = {};
        filas.forEach(fila => {
            const requisicionesCell = fila.cells[4];
            const badges = requisicionesCell.querySelectorAll('.badge-requisicion');
            badges.forEach(badge => {
                const req = badge.textContent.trim();
                if (!req.startsWith('+')) {  // Ignorar el badge de "+N"
                    porRequisicion[req] = (porRequisicion[req] || 0) + 1;
                }
            });
        });

        // Generar HTML del resumen
        let html = '<h3><i class="fa-solid fa-chart-pie"></i> Resumen de Resultados Filtrados</h3>';
        html += '<div class="agrupaciones-cards">';

        // Card de proveedores
        if (Object.keys(porProveedor).length > 0) {
            html += '<div class="agrupacion-card" style="background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);">';
            html += '<div class="agrupacion-header">';
            html += '<h4><i class="fa-solid fa-truck"></i> Por Proveedor</h4>';
            html += `<span class="cantidad-total">${filas.length} actas</span>`;
            html += '</div>';
            html += '<div class="desglose-container">';
            html += '<strong>Desglose:</strong>';
            html += '<div class="desglose-lista">';

            const proveedoresOrdenados = Object.entries(porProveedor)
                .sort((a, b) => b[1] - a[1])
                .slice(0, 5);

            proveedoresOrdenados.forEach(([proveedor, cantidad]) => {
                html += '<div class="desglose-item">';
                html += `<span class="desglose-label">${proveedor}</span>`;
                html += `<span class="desglose-cantidad">${cantidad}</span>`;
                html += '</div>';
            });

            if (Object.keys(porProveedor).length > 5) {
                html += '<div class="desglose-item desglose-mas">';
                html += `<span class="desglose-label">+ ${Object.keys(porProveedor).length - 5} más</span>`;
                html += '</div>';
            }

            html += '</div></div></div>';
        }

        // Card de F01
        if (Object.keys(porF01).length > 0) {
            html += '<div class="agrupacion-card" style="background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);">';
            html += '<div class="agrupacion-header">';
            html += '<h4><i class="fa-solid fa-barcode"></i> Por F01</h4>';
            html += `<span class="cantidad-total">${Object.keys(porF01).length} F01</span>`;
            html += '</div>';
            html += '<div class="desglose-container">';
            html += '<strong>Más frecuentes:</strong>';
            html += '<div class="desglose-lista">';

            const f01Ordenados = Object.entries(porF01)
                .sort((a, b) => b[1] - a[1])
                .slice(0, 5);

            f01Ordenados.forEach(([f01, cantidad]) => {
                html += '<div class="desglose-item">';
                html += `<span class="desglose-label">${f01}</span>`;
                html += `<span class="desglose-cantidad">${cantidad} actas</span>`;
                html += '</div>';
            });

            if (Object.keys(porF01).length > 5) {
                html += '<div class="desglose-item desglose-mas">';
                html += `<span class="desglose-label">+ ${Object.keys(porF01).length - 5} más</span>`;
                html += '</div>';
            }

            html += '</div></div></div>';
        }

        // Card de Orden de Compra
        if (Object.keys(porOrdenCompra).length > 0) {
            html += '<div class="agrupacion-card" style="background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);">';
            html += '<div class="agrupacion-header">';
            html += '<h4><i class="fa-solid fa-receipt"></i> Por Orden de Compra</h4>';
            html += `<span class="cantidad-total">${Object.keys(porOrdenCompra).length} órdenes</span>`;
            html += '</div>';
            html += '<div class="desglose-container">';
            html += '<strong>Más frecuentes:</strong>';
            html += '<div class="desglose-lista">';

            const ordenesOrdenadas = Object.entries(porOrdenCompra)
                .sort((a, b) => b[1] - a[1])
                .slice(0, 5);

            ordenesOrdenadas.forEach(([orden, cantidad]) => {
                html += '<div class="desglose-item">';
                html += `<span class="desglose-label">${orden}</span>`;
                html += `<span class="desglose-cantidad">${cantidad} actas</span>`;
                html += '</div>';
            });

            if (Object.keys(porOrdenCompra).length > 5) {
                html += '<div class="desglose-item desglose-mas">';
                html += `<span class="desglose-label">+ ${Object.keys(porOrdenCompra).length - 5} más</span>`;
                html += '</div>';
            }

            html += '</div></div></div>';
        }

        // Card de requisiciones
        if (Object.keys(porRequisicion).length > 0) {
            html += '<div class="agrupacion-card" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">';
            html += '<div class="agrupacion-header">';
            html += '<h4><i class="fa-solid fa-file-lines"></i> Por Requisición</h4>';
            html += `<span class="cantidad-total">${Object.keys(porRequisicion).length} requisiciones</span>`;
            html += '</div>';
            html += '<div class="desglose-container">';
            html += '<strong>Más frecuentes:</strong>';
            html += '<div class="desglose-lista">';

            const requisicionesOrdenadas = Object.entries(porRequisicion)
                .sort((a, b) => b[1] - a[1])
                .slice(0, 5);

            requisicionesOrdenadas.forEach(([req, cantidad]) => {
                html += '<div class="desglose-item">';
                html += `<span class="desglose-label">${req}</span>`;
                html += `<span class="desglose-cantidad">${cantidad} actas</span>`;
                html += '</div>';
            });

            if (Object.keys(porRequisicion).length > 5) {
                html += '<div class="desglose-item desglose-mas">';
                html += `<span class="desglose-label">+ ${Object.keys(porRequisicion).length - 5} más</span>`;
                html += '</div>';
            }

            html += '</div></div></div>';
        }

        html += '</div>';
        resumenContainer.innerHTML = html;
        resumenContainer.style.display = 'block';
    }

    // Actualizar resumen inicialmente
    actualizarResumen();

    // Actualizar cuando cambien los filtros
    const inputs = document.querySelectorAll('.panel-busqueda-avanzado input');
    inputs.forEach(input => {
        input.addEventListener('input', () => {
            setTimeout(actualizarResumen, 100);
        });
    });
}