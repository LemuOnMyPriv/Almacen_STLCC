/**
 * Inicializa la paginación para una tabla
 * @param {string} tableId - ID de la tabla
 * @param {number} registrosPorPaginaInicial - Registros por página por defecto (10, 25, 50, 100)
 */
function inicializarPaginacion(tableId, registrosPorPaginaInicial = 25) {
    const table = document.getElementById(tableId);
    if (!table) {
        console.error('No se encontró la tabla para paginación');
        return;
    }

    const tbody = table.querySelector('tbody');
    if (!tbody) {
        console.error('La tabla no tiene tbody');
        return;
    }

    let paginaActual = 1;
    let registrosPorPagina = registrosPorPaginaInicial;

    // Crear controles de paginación
    const seccionTabla = table.closest('.tabla-productos, .tabla-proveedores, .tabla-categorias, .tabla-actas, .tabla-auditoria') || table.parentElement;

    let paginacionContainer = seccionTabla.querySelector('.paginacion-container');
    if (!paginacionContainer) {
        paginacionContainer = document.createElement('div');
        paginacionContainer.className = 'paginacion-container';
        paginacionContainer.innerHTML = `
            <div class="paginacion-controles">
                <span class="paginacion-label">Mostrar:</span>
                <select class="paginacion-select" id="registrosPorPagina-${tableId}">
                    <option value="10" ${registrosPorPagina === 10 ? 'selected' : ''}>10</option>
                    <option value="25" ${registrosPorPagina === 25 ? 'selected' : ''}>25</option>
                    <option value="50" ${registrosPorPagina === 50 ? 'selected' : ''}>50</option>
                    <option value="100" ${registrosPorPagina === 100 ? 'selected' : ''}>100</option>
                </select>
                <span class="paginacion-label">registros</span>
            </div>
            <div class="paginacion-info">
                <span id="info-registros-${tableId}">Mostrando 0 de 0 registros</span>
            </div>
            <div class="paginacion-botones">
                <button class="btn-paginacion" id="btnAnterior-${tableId}">
                    <i class="fa-solid fa-chevron-left"></i> Anterior
                </button>
                <span class="pagina-actual">
                    Página <strong id="paginaActual-${tableId}">1</strong> de <strong id="totalPaginas-${tableId}">1</strong>
                </span>
                <button class="btn-paginacion" id="btnSiguiente-${tableId}">
                    Siguiente <i class="fa-solid fa-chevron-right"></i>
                </button>
            </div>
        `;
        seccionTabla.appendChild(paginacionContainer);
    }

    // Referencias a elementos
    const selectRegistros = document.getElementById(`registrosPorPagina-${tableId}`);
    const infoRegistros = document.getElementById(`info-registros-${tableId}`);
    const paginaActualSpan = document.getElementById(`paginaActual-${tableId}`);
    const totalPaginasSpan = document.getElementById(`totalPaginas-${tableId}`);
    const btnAnterior = document.getElementById(`btnAnterior-${tableId}`);
    const btnSiguiente = document.getElementById(`btnSiguiente-${tableId}`);

    // Función para obtener todas las filas visibles (sin contar mensajes)
    function obtenerFilasVisibles() {
        const filas = Array.from(tbody.getElementsByTagName('tr'));
        return filas.filter(fila =>
            !fila.classList.contains('mensaje-vacio') &&
            !fila.classList.contains('fila-sin-resultados') &&
            fila.style.display !== 'none'
        );
    }

    // Función para actualizar la paginación
    function actualizarPaginacion() {
        const filasVisibles = obtenerFilasVisibles();
        const totalRegistros = filasVisibles.length;
        const totalPaginas = Math.ceil(totalRegistros / registrosPorPagina) || 1;

        // Ajustar página actual si es necesario
        if (paginaActual > totalPaginas) {
            paginaActual = totalPaginas;
        }

        // Calcular índices
        const inicio = (paginaActual - 1) * registrosPorPagina;
        const fin = inicio + registrosPorPagina;

        // Mostrar/ocultar filas según la página actual
        filasVisibles.forEach((fila, index) => {
            if (index >= inicio && index < fin) {
                fila.style.display = '';
            } else {
                fila.style.display = 'none';
            }
        });

        // Actualizar información
        const registrosMostrados = Math.min(totalRegistros, fin) - inicio;
        infoRegistros.textContent = `Mostrando ${inicio + 1}-${Math.min(fin, totalRegistros)} de ${totalRegistros} registros`;
        paginaActualSpan.textContent = paginaActual;
        totalPaginasSpan.textContent = totalPaginas;

        // Actualizar estado de botones
        btnAnterior.disabled = paginaActual === 1;
        btnSiguiente.disabled = paginaActual === totalPaginas;

        // Mostrar/ocultar contenedor de paginación
        if (totalRegistros === 0) {
            paginacionContainer.style.display = 'none';
        } else {
            paginacionContainer.style.display = 'flex';
        }
    }

    // Event listeners
    selectRegistros.addEventListener('change', function () {
        registrosPorPagina = parseInt(this.value);
        paginaActual = 1;
        actualizarPaginacion();
    });

    btnAnterior.addEventListener('click', function () {
        if (paginaActual > 1) {
            paginaActual--;
            actualizarPaginacion();
        }
    });

    btnSiguiente.addEventListener('click', function () {
        const filasVisibles = obtenerFilasVisibles();
        const totalPaginas = Math.ceil(filasVisibles.length / registrosPorPagina);
        if (paginaActual < totalPaginas) {
            paginaActual++;
            actualizarPaginacion();
        }
    });

    // Observar cambios en la tabla (cuando se aplican filtros)
    const observer = new MutationObserver(function () {
        paginaActual = 1; // Reiniciar a página 1 cuando cambien los filtros
        actualizarPaginacion();
    });

    observer.observe(tbody, {
        childList: true,
        attributes: true,
        attributeFilter: ['style'],
        subtree: true
    });

    // Inicializar
    actualizarPaginacion();

    // Retornar función para actualizar manualmente si es necesario
    return {
        actualizar: actualizarPaginacion,
        irAPagina: function (pagina) {
            paginaActual = pagina;
            actualizarPaginacion();
        }
    };
}