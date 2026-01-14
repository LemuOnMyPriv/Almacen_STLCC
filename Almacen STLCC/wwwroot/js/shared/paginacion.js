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

    // Variable para evitar bucles infinitos
    let actualizandoPaginacion = false;
    let totalRegistrosAnterior = 0;

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

    // Función para obtener todas las filas disponibles (sin contar mensajes)
    // NO filtrar por display:none porque la paginación usa eso
    function obtenerTodasLasFilas() {
        const filas = Array.from(tbody.getElementsByTagName('tr'));
        return filas.filter(fila =>
            !fila.classList.contains('mensaje-vacio') &&
            !fila.classList.contains('fila-sin-resultados')
        );
    }

    // Función para obtener solo filas visibles por filtros (no por paginación)
    function obtenerFilasVisiblesPorFiltro() {
        const filas = Array.from(tbody.getElementsByTagName('tr'));
        return filas.filter(fila => {
            if (fila.classList.contains('mensaje-vacio') ||
                fila.classList.contains('fila-sin-resultados')) {
                return false;
            }

            // Si tiene el atributo data-oculto-filtro, está oculta por filtro
            return !fila.hasAttribute('data-oculto-filtro');
        });
    }

    // Función para actualizar la paginación
    function actualizarPaginacion() {
        actualizandoPaginacion = true; // Activar flag

        const filasVisibles = obtenerFilasVisiblesPorFiltro();
        const totalRegistros = filasVisibles.length;
        const totalPaginas = Math.ceil(totalRegistros / registrosPorPagina) || 1;

        // Ajustar página actual si es necesario
        if (paginaActual > totalPaginas) {
            paginaActual = totalPaginas;
        }
        if (paginaActual < 1) {
            paginaActual = 1;
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

        // Actualizar información con el cálculo correcto
        const registroInicio = totalRegistros === 0 ? 0 : inicio + 1;
        const registroFin = Math.min(fin, totalRegistros);

        infoRegistros.textContent = `Mostrando ${registroInicio}-${registroFin} de ${totalRegistros} registros`;
        paginaActualSpan.textContent = paginaActual;
        totalPaginasSpan.textContent = totalPaginas;

        // Actualizar estado de botones
        btnAnterior.disabled = paginaActual === 1;
        btnSiguiente.disabled = paginaActual >= totalPaginas;

        // Mostrar/ocultar contenedor de paginación
        if (totalRegistros === 0) {
            paginacionContainer.style.display = 'none';
        } else {
            paginacionContainer.style.display = 'flex';
        }

        // Guardar el total actual para el observer
        totalRegistrosAnterior = totalRegistros;

        // Desactivar flag después de un breve delay
        setTimeout(() => {
            actualizandoPaginacion = false;
        }, 100);
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
        console.log('🔵 Clic en Siguiente');
        console.log('Página actual:', paginaActual);
        console.log('Registros por página:', registrosPorPagina);

        const filasVisibles = obtenerFilasVisiblesPorFiltro();
        console.log('Filas visibles:', filasVisibles.length);

        const totalPaginas = Math.ceil(filasVisibles.length / registrosPorPagina);
        console.log('Total páginas:', totalPaginas);
        console.log('Botón deshabilitado?', btnSiguiente.disabled);

        if (paginaActual < totalPaginas) {
            console.log('✅ Incrementando página');
            paginaActual++;
            actualizarPaginacion();
        } else {
            console.log('❌ No se puede avanzar');
        }
    });

    // Observar cambios en la tabla (cuando se aplican filtros)
    const observer = new MutationObserver(function (mutations) {
        // Ignorar cambios causados por la propia paginación
        if (actualizandoPaginacion) return;

        // Obtener el total de filas visibles actual
        const totalFilasActual = obtenerFilasVisiblesPorFiltro().length;

        // Solo reiniciar si cambió el TOTAL de filas (filtros aplicados)
        // NO si solo cambiaron las filas mostradas (paginación)
        if (totalFilasActual !== totalRegistrosAnterior) {
            totalRegistrosAnterior = totalFilasActual;
            paginaActual = 1;
            actualizarPaginacion();
        }
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