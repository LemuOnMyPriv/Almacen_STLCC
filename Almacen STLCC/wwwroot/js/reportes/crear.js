// Configuración de columnas por tabla
const columnasDisponibles = {
    productos: ['Código', 'Nombre', 'Marca', 'Categoría', 'Unidad de Medida', 'Proveedores'],
    proveedores: ['Nombre', 'RTN'],
    actas: ['Numero de Acta', 'F01', 'Orden de Compra', 'Proveedor', 'Fecha', 'Productos'],
    movimientos: ['Fecha', 'Producto', 'Tipo', 'Cantidad', 'Acta']
};

const iconosTablas = {
    productos: 'fa-box',
    proveedores: 'fa-truck',
    actas: 'fa-file-lines',
    movimientos: 'fa-arrows-rotate'
};

// Estado del formulario
let pasoActual = 1;
let tablasSeleccionadas = [];
let columnasSeleccionadas = {};
let filtrosSeleccionados = {};
let formatoSeleccionado = 'excel';

document.addEventListener("DOMContentLoaded", function () {
    inicializarEventos();
});

function inicializarEventos() {
    // Paso 1: Selección de tablas
    document.querySelectorAll('input[name="tabla"]').forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            tablasSeleccionadas = Array.from(document.querySelectorAll('input[name="tabla"]:checked'))
                .map(cb => cb.value);

            document.getElementById('btnSiguiente1').disabled = tablasSeleccionadas.length === 0;
        });
    });

    document.getElementById('btnSiguiente1').addEventListener('click', () => irAPaso(2));

    // Paso 2: Botones
    document.getElementById('btnAtras2').addEventListener('click', () => irAPaso(1));
    document.getElementById('btnSiguiente2').addEventListener('click', () => irAPaso(3));

    // Paso 3: Botones
    document.getElementById('btnAtras3').addEventListener('click', () => irAPaso(2));
    document.getElementById('btnSiguiente3').addEventListener('click', () => irAPaso(4));

    // Paso 4: Botones
    document.getElementById('btnAtras4').addEventListener('click', () => irAPaso(3));
    document.getElementById('btnSiguiente4').addEventListener('click', () => irAPaso(5));

    // Paso 5: Formato
    document.querySelectorAll('input[name="formato"]').forEach(radio => {
        radio.addEventListener('change', function () {
            formatoSeleccionado = this.value;
            actualizarResumen();
        });
    });

    document.getElementById('btnAtras5').addEventListener('click', () => irAPaso(4));
    document.getElementById('btnGenerar').addEventListener('click', generarReporte);
}

function irAPaso(numeroPaso) {
    // Ocultar pregunta actual
    document.getElementById(`pregunta${pasoActual}`).classList.remove('activa');

    // Actualizar pasos en el indicador
    document.getElementById(`step${pasoActual}`).classList.remove('activo');
    document.getElementById(`step${pasoActual}`).classList.add('completado');

    pasoActual = numeroPaso;

    // Mostrar nueva pregunta
    document.getElementById(`pregunta${numeroPaso}`).classList.add('activa');
    document.getElementById(`step${numeroPaso}`).classList.add('activo');

    // Lógica específica por paso
    if (numeroPaso === 2) {
        generarSelectorColumnas();
    } else if (numeroPaso === 3) {
        generarFiltrosDinamicos();
    } else if (numeroPaso === 5) {
        actualizarResumen();
    }

    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function generarSelectorColumnas() {
    const container = document.getElementById('columnasContainer');
    container.innerHTML = '';

    tablasSeleccionadas.forEach(tabla => {
        const tablaDiv = document.createElement('div');
        tablaDiv.className = 'tabla-columnas';

        const nombreTabla = tabla.charAt(0).toUpperCase() + tabla.slice(1);
        const icono = iconosTablas[tabla];

        tablaDiv.innerHTML = `
            <h3>
                <i class="fa-solid ${icono}"></i>
                ${nombreTabla}
            </h3>
            <div class="columnas-grid" id="columnas-${tabla}"></div>
        `;

        container.appendChild(tablaDiv);

        // Generar checkboxes de columnas
        const columnasGrid = document.getElementById(`columnas-${tabla}`);
        columnasDisponibles[tabla].forEach(columna => {
            const colDiv = document.createElement('div');
            colDiv.className = 'columna-checkbox';
            colDiv.innerHTML = `
                <input type="checkbox" 
                       id="col-${tabla}-${columna}" 
                       value="${columna}"
                       data-tabla="${tabla}" />
                <label for="col-${tabla}-${columna}">${columna}</label>
            `;
            columnasGrid.appendChild(colDiv);

            // Event listener
            colDiv.querySelector('input').addEventListener('change', validarColumnas);
        });
    });

    validarColumnas();
}

function validarColumnas() {
    columnasSeleccionadas = {};
    let haySeleccion = false;

    tablasSeleccionadas.forEach(tabla => {
        const checkboxes = document.querySelectorAll(`input[data-tabla="${tabla}"]:checked`);
        columnasSeleccionadas[tabla] = Array.from(checkboxes).map(cb => cb.value);

        if (columnasSeleccionadas[tabla].length > 0) {
            haySeleccion = true;
        }
    });

    document.getElementById('btnSiguiente2').disabled = !haySeleccion;
}

function generarFiltrosDinamicos() {
    const container = document.getElementById('filtrosContainer');
    container.innerHTML = '';

    // Nota informativa
    const nota = document.createElement('div');
    nota.className = 'filtro-nota';
    nota.innerHTML = `
        <i class="fa-solid fa-info-circle"></i>
        Los filtros son opcionales. Si no completa ningún filtro, se incluirán todos los registros.
    `;
    container.appendChild(nota);

    // Generar filtros por cada tabla seleccionada
    tablasSeleccionadas.forEach(tabla => {
        const tablaDiv = document.createElement('div');
        tablaDiv.className = 'filtros-tabla';

        const nombreTabla = tabla.charAt(0).toUpperCase() + tabla.slice(1);
        const icono = iconosTablas[tabla];

        tablaDiv.innerHTML = `
            <h3>
                <i class="fa-solid ${icono}"></i>
                Filtros de ${nombreTabla}
            </h3>
            <div class="filtros-tabla-grid" id="filtros-${tabla}"></div>
        `;

        container.appendChild(tablaDiv);

        const filtrosGrid = document.getElementById(`filtros-${tabla}`);

        // Generar inputs de filtro para cada columna seleccionada
        if (columnasSeleccionadas[tabla]) {
            columnasSeleccionadas[tabla].forEach(columna => {
                const filtroDiv = document.createElement('div');
                filtroDiv.className = 'filtro-item';

                const inputId = `filtro-${tabla}-${columna.replace(/ /g, '-')}`;

                filtroDiv.innerHTML = `
                    <label for="${inputId}">
                        <i class="fa-solid fa-filter"></i> ${columna}:
                    </label>
                    <input type="text" 
                           id="${inputId}" 
                           class="form-control-filtro"
                           placeholder="Filtrar por ${columna.toLowerCase()}..."
                           data-tabla="${tabla}"
                           data-columna="${columna}" />
                `;

                filtrosGrid.appendChild(filtroDiv);

                // Event listener para guardar filtros
                filtroDiv.querySelector('input').addEventListener('input', function () {
                    if (!filtrosSeleccionados[tabla]) {
                        filtrosSeleccionados[tabla] = {};
                    }

                    if (this.value.trim()) {
                        filtrosSeleccionados[tabla][columna] = this.value.trim();
                    } else {
                        delete filtrosSeleccionados[tabla][columna];
                    }
                });
            });
        }
    });
}

function actualizarResumen() {
    const container = document.getElementById('resumenContenido');
    container.innerHTML = '';

    // Tablas seleccionadas
    const tablasItem = document.createElement('div');
    tablasItem.className = 'resumen-item';
    tablasItem.innerHTML = `
        <i class="fa-solid fa-table"></i>
        <div class="resumen-item-content">
            <div><strong>Tablas:</strong></div>
            <div class="resumen-badges">
                ${tablasSeleccionadas.map(t => `
                    <span class="resumen-badge">
                        <i class="fa-solid ${iconosTablas[t]}"></i>
                        ${t.charAt(0).toUpperCase() + t.slice(1)}
                    </span>
                `).join('')}
            </div>
        </div>
    `;
    container.appendChild(tablasItem);

    // Columnas por tabla
    Object.entries(columnasSeleccionadas).forEach(([tabla, columnas]) => {
        if (columnas.length > 0) {
            const colItem = document.createElement('div');
            colItem.className = 'resumen-item';
            colItem.innerHTML = `
                <i class="fa-solid fa-columns"></i>
                <div class="resumen-item-content">
                    <div><strong>${tabla.charAt(0).toUpperCase() + tabla.slice(1)}:</strong></div>
                    <div class="resumen-badges">
                        ${columnas.map(c => `<span class="resumen-badge">${c}</span>`).join('')}
                    </div>
                </div>
            `;
            container.appendChild(colItem);
        }
    });

    // Filtros aplicados
    const hayFiltros = Object.keys(filtrosSeleccionados).some(tabla =>
        Object.keys(filtrosSeleccionados[tabla] || {}).length > 0
    );

    if (hayFiltros) {
        Object.entries(filtrosSeleccionados).forEach(([tabla, filtros]) => {
            if (Object.keys(filtros).length > 0) {
                const filtrosItem = document.createElement('div');
                filtrosItem.className = 'resumen-item';
                filtrosItem.innerHTML = `
                    <i class="fa-solid fa-filter"></i>
                    <div class="resumen-item-content">
                        <div><strong>Filtros de ${tabla.charAt(0).toUpperCase() + tabla.slice(1)}:</strong></div>
                        <div class="resumen-badges">
                            ${Object.entries(filtros).map(([col, val]) =>
                    `<span class="resumen-badge">${col}: "${val}"</span>`
                ).join('')}
                        </div>
                    </div>
                `;
                container.appendChild(filtrosItem);
            }
        });
    }

    // Formato
    const formatoItem = document.createElement('div');
    formatoItem.className = 'resumen-item';
    const iconoFormato = formatoSeleccionado === 'excel' ? 'fa-file-excel' :
        formatoSeleccionado === 'pdf' ? 'fa-file-pdf' : 'fa-file-word';
    formatoItem.innerHTML = `
        <i class="fa-solid ${iconoFormato}"></i>
        <div class="resumen-item-content">
            <strong>Formato:</strong> ${formatoSeleccionado.toUpperCase()}
        </div>
    `;
    container.appendChild(formatoItem);
}

async function generarReporte() {
    try {
        // Mostrar loading
        document.getElementById('loadingOverlay').style.display = 'flex';

        const payload = {
            tablas: tablasSeleccionadas,
            columnas: columnasSeleccionadas,
            filtros: filtrosSeleccionados,
            formato: formatoSeleccionado
        };

        console.log('Enviando payload:', payload);

        const response = await fetch('/Reportes/Crear?handler=Generar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('Error del servidor:', errorText);
            throw new Error(`Error del servidor: ${response.status}`);
        }

        // Descargar archivo
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;

        const extension = formatoSeleccionado === 'excel' ? 'xlsx' :
            formatoSeleccionado === 'pdf' ? 'pdf' : 'docx';
        a.download = `reporte_${new Date().getTime()}.${extension}`;

        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        // Ocultar loading
        document.getElementById('loadingOverlay').style.display = 'none';

        // Mensaje de éxito
        Swal.fire({
            icon: 'success',
            title: '¡Reporte generado!',
            text: 'Su reporte se ha descargado correctamente',
            confirmButtonText: 'Aceptar'
        });

    } catch (error) {
        console.error('Error completo:', error);
        document.getElementById('loadingOverlay').style.display = 'none';

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Ocurrió un error al generar el reporte: ' + error.message,
            confirmButtonText: 'Aceptar'
        });
    }
}