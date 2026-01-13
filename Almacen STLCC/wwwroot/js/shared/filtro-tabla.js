// ============================================
// FILTRO EN TIEMPO REAL PARA TABLAS
// Guardar como: wwwroot/js/shared/filtro-tabla.js
// ESTE ARCHIVO SE IMPORTA EN _Layout.cshtml
// ============================================

/**
 * Inicializa el filtrado en tiempo real para una tabla
 * @param {string} inputId - ID del input de búsqueda
 * @param {string} tableId - ID de la tabla a filtrar
 * @param {number[]} columnasAFiltrar - Índices de las columnas a incluir en la búsqueda (base 0)
 */
function inicializarFiltroTabla(inputId, tableId, columnasAFiltrar = null) {
    const input = document.getElementById(inputId);
    const table = document.getElementById(tableId);

    if (!input || !table) {
        console.error('No se encontró el input o la tabla');
        return;
    }

    const tbody = table.querySelector('tbody');
    if (!tbody) {
        console.error('La tabla no tiene tbody');
        return;
    }

    // Contador de resultados
    let contadorResultados = document.getElementById('contadorResultados');
    if (!contadorResultados) {
        // Crear contador si no existe
        contadorResultados = document.createElement('div');
        contadorResultados.id = 'contadorResultados';
        contadorResultados.className = 'contador-resultados';

        // Insertar ANTES de la tabla
        const seccionTabla = table.closest('.tabla-productos, .tabla-proveedores, .tabla-categorias, .tabla-actas, .tabla-auditoria') || table.parentElement;
        seccionTabla.insertBefore(contadorResultados, seccionTabla.firstChild);
    }

    // Función de filtrado
    function filtrarTabla() {
        const filtro = input.value.toLowerCase().trim();
        const filas = tbody.getElementsByTagName('tr');
        let filasVisibles = 0;
        let totalFilas = 0;

        for (let i = 0; i < filas.length; i++) {
            const fila = filas[i];

            // Ignorar filas de "sin datos" o mensajes vacíos
            if (fila.classList.contains('mensaje-vacio') ||
                fila.querySelector('.mensaje-vacio') ||
                fila.classList.contains('fila-sin-resultados')) {
                continue;
            }

            totalFilas++;
            const celdas = fila.getElementsByTagName('td');
            let textoFila = '';

            // Si se especificaron columnas, solo buscar en esas
            if (columnasAFiltrar && columnasAFiltrar.length > 0) {
                columnasAFiltrar.forEach(indice => {
                    if (celdas[indice]) {
                        textoFila += ' ' + celdas[indice].textContent;
                    }
                });
            } else {
                // Si no, buscar en todas las columnas (excepto acciones)
                for (let j = 0; j < celdas.length; j++) {
                    // Ignorar columna de acciones
                    if (!celdas[j].classList.contains('acciones')) {
                        textoFila += ' ' + celdas[j].textContent;
                    }
                }
            }

            textoFila = textoFila.toLowerCase();

            // Mostrar u ocultar fila según coincida con el filtro
            if (filtro === '' || textoFila.includes(filtro)) {
                fila.style.display = '';
                filasVisibles++;
            } else {
                fila.style.display = 'none';
            }
        }

        // Actualizar contador
        actualizarContador(filasVisibles, totalFilas);

        // Mostrar mensaje si no hay resultados
        mostrarMensajeNoResultados(filasVisibles, tbody);
    }

    // Actualizar contador de resultados
    function actualizarContador(visibles, total) {
        if (input.value.trim() === '') {
            contadorResultados.style.display = 'none';
            contadorResultados.innerHTML = '';
            return;
        }

        contadorResultados.style.display = 'block';
        contadorResultados.innerHTML = `
            <i class="fa-solid fa-filter"></i> 
            Mostrando <strong>${visibles}</strong> de <strong>${total}</strong> registros
        `;
    }

    // Mostrar mensaje cuando no hay resultados - COLSPAN DINÁMICO
    function mostrarMensajeNoResultados(visibles, tbody) {
        let mensajeNoResultados = tbody.querySelector('.fila-sin-resultados');

        if (visibles === 0 && input.value.trim() !== '') {
            if (!mensajeNoResultados) {
                // Calcular colspan dinámicamente
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

    // Event listener para filtrado en tiempo real
    input.addEventListener('input', filtrarTabla);

    // Limpiar filtro con el botón X (si existe)
    const btnLimpiar = document.getElementById('btnLimpiarFiltro');
    if (btnLimpiar) {
        btnLimpiar.addEventListener('click', function () {
            input.value = '';
            filtrarTabla();
            input.focus();
        });
    }
}

/**
 * Inicializa filtrado múltiple (varios inputs para diferentes columnas)
 * @param {Object} config - Configuración de filtros { inputId: columnIndex }
 * @param {string} tableId - ID de la tabla
 * @param {Object} fechaConfig - Opcional: { columnaFecha: number, inputDesde: string, inputHasta: string }
 */
function inicializarFiltroMultiple(config, tableId, fechaConfig = null) {
    const table = document.getElementById(tableId);
    if (!table) {
        console.error('No se encontró la tabla');
        return;
    }

    const tbody = table.querySelector('tbody');
    if (!tbody) {
        console.error('La tabla no tiene tbody');
        return;
    }

    const inputs = {};
    let fechaInputs = null;

    // Obtener todos los inputs de texto/select
    for (const [inputId, columnIndex] of Object.entries(config)) {
        const input = document.getElementById(inputId);
        if (input) {
            inputs[inputId] = { element: input, column: columnIndex };
        }
    }

    // Configurar inputs de fecha si existen
    if (fechaConfig) {
        const inputDesde = document.getElementById(fechaConfig.inputDesde);
        const inputHasta = document.getElementById(fechaConfig.inputHasta);
        if (inputDesde && inputHasta) {
            fechaInputs = {
                desde: inputDesde,
                hasta: inputHasta,
                columna: fechaConfig.columnaFecha
            };
        }
    }

    // Función de filtrado múltiple
    function filtrarTablaMultiple() {
        const filas = tbody.getElementsByTagName('tr');
        let filasVisibles = 0;
        let totalFilas = 0;

        for (let i = 0; i < filas.length; i++) {
            const fila = filas[i];

            // Ignorar filas de mensajes
            if (fila.classList.contains('mensaje-vacio') ||
                fila.querySelector('.mensaje-vacio') ||
                fila.classList.contains('fila-sin-resultados')) {
                continue;
            }

            totalFilas++;
            const celdas = fila.getElementsByTagName('td');
            let mostrarFila = true;

            // Verificar filtros de texto/select
            for (const [inputId, data] of Object.entries(inputs)) {
                const filtro = data.element.value.toLowerCase().trim();

                if (filtro !== '') {
                    const celda = celdas[data.column];
                    if (celda) {
                        const textoCelda = celda.textContent.toLowerCase();
                        if (!textoCelda.includes(filtro)) {
                            mostrarFila = false;
                            break;
                        }
                    } else {
                        mostrarFila = false;
                        break;
                    }
                }
            }

            // Verificar filtro de fechas si está configurado
            if (mostrarFila && fechaInputs) {
                const fechaDesde = fechaInputs.desde.value;
                const fechaHasta = fechaInputs.hasta.value;

                if (fechaDesde || fechaHasta) {
                    const celdaFecha = celdas[fechaInputs.columna];
                    if (celdaFecha) {
                        const textoFecha = celdaFecha.textContent.trim();
                        const fechaFila = parsearFecha(textoFecha);

                        if (fechaFila) {
                            if (fechaDesde) {
                                const desde = new Date(fechaDesde);
                                if (fechaFila < desde) {
                                    mostrarFila = false;
                                }
                            }
                            if (fechaHasta && mostrarFila) {
                                const hasta = new Date(fechaHasta);
                                hasta.setHours(23, 59, 59, 999); // Incluir todo el día
                                if (fechaFila > hasta) {
                                    mostrarFila = false;
                                }
                            }
                        }
                    }
                }
            }

            // Mostrar u ocultar fila
            if (mostrarFila) {
                fila.style.display = '';
                filasVisibles++;
            } else {
                fila.style.display = 'none';
            }
        }

        // Actualizar contador
        actualizarContadorMultiple(filasVisibles, totalFilas);

        // Mostrar mensaje si no hay resultados
        mostrarMensajeNoResultados(filasVisibles, tbody);
    }

    // Función auxiliar para parsear fechas en formato dd/MM/yyyy o dd/MM/yyyy HH:mm:ss
    function parsearFecha(textoFecha) {
        // Formato: dd/MM/yyyy o dd/MM/yyyy HH:mm:ss
        const partes = textoFecha.split(' ')[0].split('/');
        if (partes.length === 3) {
            const dia = parseInt(partes[0]);
            const mes = parseInt(partes[1]) - 1; // Los meses en JS van de 0-11
            const año = parseInt(partes[2]);
            return new Date(año, mes, dia);
        }
        return null;
    }

    function actualizarContadorMultiple(visibles, total) {
        let contadorResultados = document.getElementById('contadorResultadosMultiple');

        // Verificar si hay algún filtro activo (incluyendo fechas)
        const hayFiltrosActivos = Object.values(inputs).some(
            data => data.element.value.trim() !== ''
        ) || (fechaInputs && (fechaInputs.desde.value || fechaInputs.hasta.value));

        if (!hayFiltrosActivos) {
            if (contadorResultados) {
                contadorResultados.style.display = 'none';
                contadorResultados.innerHTML = '';
            }
            return;
        }

        if (!contadorResultados) {
            contadorResultados = document.createElement('div');
            contadorResultados.id = 'contadorResultadosMultiple';
            contadorResultados.className = 'contador-resultados';

            // Insertar ANTES de la tabla
            const seccionTabla = table.closest('.tabla-productos, .tabla-proveedores, .tabla-categorias, .tabla-actas, .tabla-auditoria') || table.parentElement;
            const primeraTabla = seccionTabla.querySelector('table');
            if (primeraTabla) {
                seccionTabla.insertBefore(contadorResultados, primeraTabla);
            } else {
                seccionTabla.insertBefore(contadorResultados, seccionTabla.firstChild);
            }
        }

        contadorResultados.style.display = 'flex';
        contadorResultados.innerHTML = `
            <i class="fa-solid fa-filter"></i> 
            Mostrando <strong>${visibles}</strong> de <strong>${total}</strong> registros
        `;
    }

    // Agregar event listeners a todos los inputs (texto/select y fechas)
    for (const data of Object.values(inputs)) {
        data.element.addEventListener('input', filtrarTablaMultiple);
    }

    if (fechaInputs) {
        fechaInputs.desde.addEventListener('change', filtrarTablaMultiple);
        fechaInputs.hasta.addEventListener('change', filtrarTablaMultiple);
    }

    // COLSPAN DINÁMICO con clase filtro-vacio
    function mostrarMensajeNoResultados(visibles, tbody) {
        let mensajeNoResultados = tbody.querySelector('.fila-sin-resultados');

        // Verificar si hay algún filtro activo (incluyendo fechas)
        const hayFiltrosActivos = Object.values(inputs).some(
            data => data.element.value.trim() !== ''
        ) || (fechaInputs && (fechaInputs.desde.value || fechaInputs.hasta.value));

        if (visibles === 0 && hayFiltrosActivos) {
            if (!mensajeNoResultados) {
                // Calcular colspan dinámicamente
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
}