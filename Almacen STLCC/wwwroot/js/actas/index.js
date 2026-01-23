document.addEventListener("DOMContentLoaded", function () {
    const tabla = document.getElementById('tablaActas');
    if (!tabla) return;

    const tbody = tabla.querySelector('tbody');
    
    // Todos los inputs de filtro
    const filtros = {
        'filtroNumeroActa': 0,
        'filtroF01': 1,
        'filtroOrdenCompra': 2,
        'filtroProveedor': 3,
        'filtroRequisicion': 4, // Este es especial, usa data-attribute
        'filtroFechaDesde': 5,
        'filtroFechaHasta': 5
    };

    // Función principal de filtrado
    function aplicarTodosFiltros() {
        const filas = tbody.getElementsByTagName('tr');
        let filasVisibles = 0;
        let totalFilas = 0;

        for (let i = 0; i < filas.length; i++) {
            const fila = filas[i];

            // Ignorar filas especiales
            if (fila.classList.contains('mensaje-vacio') ||
                fila.classList.contains('fila-sin-resultados')) {
                continue;
            }

            totalFilas++;
            let mostrarFila = true;

            // 1. Filtrar por Número de Acta
            const filtroNumeroActa = document.getElementById('filtroNumeroActa').value.toLowerCase().trim();
            if (filtroNumeroActa && !fila.cells[0].textContent.toLowerCase().includes(filtroNumeroActa)) {
                mostrarFila = false;
            }

            // 2. Filtrar por F01
            const filtroF01 = document.getElementById('filtroF01').value.toLowerCase().trim();
            if (mostrarFila && filtroF01 && !fila.cells[1].textContent.toLowerCase().includes(filtroF01)) {
                mostrarFila = false;
            }

            // 3. Filtrar por Orden de Compra
            const filtroOrdenCompra = document.getElementById('filtroOrdenCompra').value.toLowerCase().trim();
            if (mostrarFila && filtroOrdenCompra && !fila.cells[2].textContent.toLowerCase().includes(filtroOrdenCompra)) {
                mostrarFila = false;
            }

            // 4. Filtrar por Proveedor
            const filtroProveedor = document.getElementById('filtroProveedor').value.toLowerCase().trim();
            if (mostrarFila && filtroProveedor && !fila.cells[3].textContent.toLowerCase().includes(filtroProveedor)) {
                mostrarFila = false;
            }

            // 5. Filtrar por Requisición (usando data-attribute)
            const filtroRequisicion = document.getElementById('filtroRequisicion').value.toLowerCase().trim();
            if (mostrarFila && filtroRequisicion) {
                const requisicionesData = fila.getAttribute('data-requisiciones') || '';
                const requisiciones = requisicionesData.toLowerCase();
                
                // Solo mostrar si el filtro está contenido en las requisiciones
                if (!requisiciones.includes(filtroRequisicion)) {
                    mostrarFila = false;
                }
            }

            // 6. Filtrar por fecha
            const filtroFechaDesde = document.getElementById('filtroFechaDesde').value;
            const filtroFechaHasta = document.getElementById('filtroFechaHasta').value;
            
            if (mostrarFila && (filtroFechaDesde || filtroFechaHasta)) {
                const textoFecha = fila.cells[5].textContent.trim();
                const fechaFila = parsearFecha(textoFecha);

                if (fechaFila) {
                    if (filtroFechaDesde) {
                        const desde = new Date(filtroFechaDesde);
                        if (fechaFila < desde) {
                            mostrarFila = false;
                        }
                    }
                    if (filtroFechaHasta && mostrarFila) {
                        const hasta = new Date(filtroFechaHasta);
                        hasta.setHours(23, 59, 59, 999);
                        if (fechaFila > hasta) {
                            mostrarFila = false;
                        }
                    }
                }
            }

            // Aplicar visibilidad
            if (mostrarFila) {
                fila.style.display = '';
                fila.removeAttribute('data-oculto-filtro');
                filasVisibles++;
            } else {
                fila.style.display = 'none';
                fila.setAttribute('data-oculto-filtro', 'true');
            }
        }

        actualizarContador(filasVisibles, totalFilas);
        mostrarMensajeSinResultados(filasVisibles, tbody);
        
        // Actualizar resumen si existe
        if (typeof actualizarResumen === 'function') {
            actualizarResumen();
        }
    }

    // Función auxiliar para parsear fechas dd/MM/yyyy
    function parsearFecha(textoFecha) {
        const partes = textoFecha.split('/');
        if (partes.length === 3) {
            const dia = parseInt(partes[0]);
            const mes = parseInt(partes[1]) - 1;
            const año = parseInt(partes[2]);
            return new Date(año, mes, dia);
        }
        return null;
    }

    // Función para actualizar contador
    function actualizarContador(visibles, total) {
        let contadorResultados = document.getElementById('contadorResultadosMultiple');
        
        const hayFiltrosActivos = Array.from(document.querySelectorAll('.panel-busqueda-avanzado input')).some(
            input => input.value.trim() !== ''
        );

        if (!hayFiltrosActivos) {
            if (contadorResultados) {
                contadorResultados.style.display = 'none';
            }
            return;
        }

        if (!contadorResultados) {
            contadorResultados = document.createElement('div');
            contadorResultados.id = 'contadorResultadosMultiple';
            contadorResultados.className = 'contador-resultados';

            const seccionTabla = tabla.closest('.tabla-actas') || tabla.parentElement;
            const primeraTabla = seccionTabla.querySelector('table');
            if (primeraTabla) {
                seccionTabla.insertBefore(contadorResultados, primeraTabla);
            }
        }

        contadorResultados.style.display = 'flex';
        contadorResultados.innerHTML = `
            <i class="fa-solid fa-filter"></i> 
            Mostrando <strong>${visibles}</strong> de <strong>${total}</strong> registros
        `;
    }

    // Función para mostrar mensaje sin resultados
    function mostrarMensajeSinResultados(visibles, tbody) {
        let mensajeNoResultados = tbody.querySelector('.fila-sin-resultados');

        const hayFiltrosActivos = Array.from(document.querySelectorAll('.panel-busqueda-avanzado input')).some(
            input => input.value.trim() !== ''
        );

        if (visibles === 0 && hayFiltrosActivos) {
            if (!mensajeNoResultados) {
                const numColumnas = tbody.closest('table').querySelectorAll('thead th').length;

                mensajeNoResultados = document.createElement('tr');
                mensajeNoResultados.className = 'fila-sin-resultados';
                mensajeNoResultados.innerHTML = `
                    <td colspan="${numColumnas}">
                        <div class="filtro-vacio">
                            <i class="fa-solid fa-magnifying-glass"></i> 
                            No se encontraron resultados con los filtros aplicados
                        </div>
                    </td>
                `;
                tbody.appendChild(mensajeNoResultados);
            }
        } else {
            if (mensajeNoResultados) {
                mensajeNoResultados.remove();
            }
        }
    }

    // Agregar event listeners a TODOS los filtros
    const todosLosFiltros = document.querySelectorAll('.panel-busqueda-avanzado input');
    todosLosFiltros.forEach(input => {
        input.addEventListener('input', aplicarTodosFiltros);
        input.addEventListener('change', aplicarTodosFiltros);
    });

    // Inicializar sistema de agrupación
    inicializarAgrupacionActas();

    // Inicializar paginación
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
    window.actualizarResumen = function() {
        // Solo mostrar si hay filtros activos
        if (!hayFiltrosActivos()) {
            resumenContainer.style.display = 'none';
            return;
        }

        const filas = Array.from(tbody.getElementsByTagName('tr')).filter(fila =>
            !fila.classList.contains('mensaje-vacio') &&
            !fila.classList.contains('fila-sin-resultados') &&
            fila.style.display !== 'none' &&
            !fila.hasAttribute('data-oculto-filtro')
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
            const requisicionesData = fila.getAttribute('data-requisiciones') || '';
            if (requisicionesData) {
                const requisiciones = requisicionesData.split(',').filter(r => r.trim() !== '');
                requisiciones.forEach(req => {
                    porRequisicion[req] = (porRequisicion[req] || 0) + 1;
                });
            }
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