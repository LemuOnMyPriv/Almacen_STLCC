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
        input.parentElement.appendChild(contadorResultados);
    }

    // Función de filtrado
    function filtrarTabla() {
        const filtro = input.value.toLowerCase().trim();
        const filas = tbody.getElementsByTagName('tr');
        let filasVisibles = 0;

        for (let i = 0; i < filas.length; i++) {
            const fila = filas[i];

            // Ignorar filas de "sin datos" o mensajes vacíos
            if (fila.classList.contains('mensaje-vacio') || fila.querySelector('.mensaje-vacio')) {
                continue;
            }

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
        actualizarContador(filasVisibles, filas.length);

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

    // Mostrar mensaje cuando no hay resultados
    function mostrarMensajeNoResultados(visibles, tbody) {
        let mensajeNoResultados = tbody.querySelector('.fila-sin-resultados');

        if (visibles === 0 && input.value.trim() !== '') {
            if (!mensajeNoResultados) {
                mensajeNoResultados = document.createElement('tr');
                mensajeNoResultados.className = 'fila-sin-resultados';
                mensajeNoResultados.innerHTML = `
                    <td colspan="100" class="mensaje-vacio">
                        <i class="fa-solid fa-magnifying-glass"></i> 
                        No se encontraron resultados para "${input.value}"
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
 */
function inicializarFiltroMultiple(config, tableId) {
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

    // Obtener todos los inputs
    for (const [inputId, columnIndex] of Object.entries(config)) {
        const input = document.getElementById(inputId);
        if (input) {
            inputs[inputId] = { element: input, column: columnIndex };
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

            // Verificar cada filtro
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

    function actualizarContadorMultiple(visibles, total) {
        let contadorResultados = document.getElementById('contadorResultadosMultiple');

        // Verificar si hay algún filtro activo
        const hayFiltrosActivos = Object.values(inputs).some(
            data => data.element.value.trim() !== ''
        );

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

            // Insertar después del panel de búsqueda
            const panel = document.querySelector('.panel-busqueda-avanzado');
            if (panel) {
                panel.parentNode.insertBefore(contadorResultados, panel.nextSibling);
            }
        }

        contadorResultados.style.display = 'block';
        contadorResultados.innerHTML = `
            <i class="fa-solid fa-filter"></i> 
            Mostrando <strong>${visibles}</strong> de <strong>${total}</strong> registros
        `;
    }

    function mostrarMensajeNoResultados(visibles, tbody) {
        let mensajeNoResultados = tbody.querySelector('.fila-sin-resultados');

        // Verificar si hay algún filtro activo
        const hayFiltrosActivos = Object.values(inputs).some(
            data => data.element.value.trim() !== ''
        );

        if (visibles === 0 && hayFiltrosActivos) {
            if (!mensajeNoResultados) {
                mensajeNoResultados = document.createElement('tr');
                mensajeNoResultados.className = 'fila-sin-resultados';
                mensajeNoResultados.innerHTML = `
                    <td colspan="100" class="mensaje-vacio">
                        <i class="fa-solid fa-magnifying-glass"></i> 
                        No se encontraron resultados con los filtros aplicados
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

    // Agregar event listeners a todos los inputs
    for (const data of Object.values(inputs)) {
        data.element.addEventListener('input', filtrarTablaMultiple);
    }
}
