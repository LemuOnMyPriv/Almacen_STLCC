// Configuración de columnas por tabla
const columnasDisponibles = {
    productos: ['Código', 'Nombre', 'Marca', 'Categoría', 'Unidad de Medida', 'Proveedores'],
    proveedores: ['Nombre', 'RTN'],
    actas: ['Numero de Acta', 'F01', 'Orden de Compra', 'Proveedor', 'Fecha', 'Productos'],
    movimientos: ['Fecha', 'Producto', 'Tipo', 'Cantidad', 'Acta']
};

// Tipos de campos de filtro por columna
const tiposCamposFiltro = {
    productos: {
        'Código': 'number',
        'Nombre': 'text',
        'Marca': 'text',
        'Categoría': 'select',
        'Unidad de Medida': 'text',
        'Proveedores': 'select'
    },
    proveedores: {
        'Nombre': 'text',
        'RTN': 'text'
    },
    actas: {
        'Numero de Acta': 'text',
        'F01': 'text',
        'Orden de Compra': 'text',
        'Proveedor': 'select',
        'Fecha': 'date',
        'Productos': 'select'
    },
    movimientos: {
        'Fecha': 'date',
        'Producto': 'select',
        'Tipo': 'select',
        'Cantidad': 'number',
        'Acta': 'select'
    }
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

// Datos para selects dinámicos
let datosSelects = {
    categorias: [],
    proveedores: [],
    productos: [],
    actas: [],
    tiposMovimiento: ['ENTRADA', 'SALIDA', 'AJUSTE']
};

document.addEventListener("DOMContentLoaded", function () {
    inicializarEventos();
    cargarDatosSelects();
});

async function cargarDatosSelects() {
    try {
        const response = await fetch('/Reportes/Crear?handler=DatosSelects');
        if (response.ok) {
            datosSelects = await response.json();
            console.log('Datos cargados:', datosSelects);
        }
    } catch (error) {
        console.error('Error cargando datos para selects:', error);
    }
}

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

    // Paso 5: Formato y botones
    document.querySelectorAll('input[name="formato"]').forEach(radio => {
        radio.addEventListener('change', function () {
            formatoSeleccionado = this.value;
            actualizarResumen();
        });
    });

    document.getElementById('btnAtras5').addEventListener('click', () => irAPaso(4));
    document.getElementById('btnGenerar').addEventListener('click', () => generarReporte(false));
    document.getElementById('btnGuardarComo').addEventListener('click', () => generarReporte(true));

    // Hacer los pasos clicables
    for (let i = 1; i <= 5; i++) {
        const stepElement = document.getElementById(`step${i}`);
        if (stepElement) {
            stepElement.addEventListener('click', function () {
                if (this.classList.contains('completado') || this.classList.contains('activo')) {
                    const numeroPaso = parseInt(this.querySelector('.step-circle').textContent);
                    if (numeroPaso < pasoActual) {
                        irAPaso(numeroPaso);
                    }
                }
            });
        }
    }
}

function irAPaso(numeroPaso) {
    // Ocultar pregunta actual
    document.getElementById(`pregunta${pasoActual}`).classList.remove('activa');

    // Actualizar pasos en el indicador
    document.getElementById(`step${pasoActual}`).classList.remove('activo');
    if (numeroPaso > pasoActual) {
        document.getElementById(`step${pasoActual}`).classList.add('completado');
    }

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

        const columnasGrid = document.getElementById(`columnas-${tabla}`);
        columnasDisponibles[tabla].forEach(columna => {
            const colDiv = document.createElement('div');
            colDiv.className = 'columna-checkbox';
            colDiv.innerHTML = `
                <input type="checkbox" 
                       id="col-${tabla}-${columna.replace(/ /g, '-')}" 
                       value="${columna}"
                       data-tabla="${tabla}" />
                <label for="col-${tabla}-${columna.replace(/ /g, '-')}">${columna}</label>
            `;
            columnasGrid.appendChild(colDiv);

            const checkbox = colDiv.querySelector('input');
            checkbox.addEventListener('change', validarColumnas);

            // Hacer que todo el div sea clicable
            colDiv.addEventListener('click', function (e) {
                if (e.target !== checkbox) {
                    checkbox.checked = !checkbox.checked;
                    checkbox.dispatchEvent(new Event('change'));
                }
            });
        });
    });

    validarColumnas();
}

function validarColumnas() {
    columnasSeleccionadas = {};
    let todasLasTablasTienenColumnas = true;

    tablasSeleccionadas.forEach(tabla => {
        const checkboxes = document.querySelectorAll(`input[data-tabla="${tabla}"]:checked`);
        columnasSeleccionadas[tabla] = Array.from(checkboxes).map(cb => cb.value);

        // Verificar que cada tabla tenga al menos 1 columna
        if (columnasSeleccionadas[tabla].length === 0) {
            todasLasTablasTienenColumnas = false;
        }
    });

    document.getElementById('btnSiguiente2').disabled = !todasLasTablasTienenColumnas;
}

function generarFiltrosDinamicos() {
    const container = document.getElementById('filtrosContainer');
    container.innerHTML = '';

    const nota = document.createElement('div');
    nota.className = 'filtro-nota';
    nota.innerHTML = `
        <i class="fa-solid fa-info-circle"></i>
        Los filtros son opcionales. Si no completa ningún filtro, se incluirán todos los registros.
    `;
    container.appendChild(nota);

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

        if (columnasSeleccionadas[tabla]) {
            columnasSeleccionadas[tabla].forEach(columna => {
                const filtroDiv = document.createElement('div');
                filtroDiv.className = 'filtro-item';

                const inputId = `filtro-${tabla}-${columna.replace(/ /g, '-')}`;
                const tipoCampo = tiposCamposFiltro[tabla][columna];

                let inputHTML = '';

                if (tipoCampo === 'date') {
                    inputHTML = `
                        <input type="date" 
                               id="${inputId}" 
                               class="form-control-filtro"
                               data-tabla="${tabla}"
                               data-columna="${columna}" />
                    `;
                } else if (tipoCampo === 'number') {
                    inputHTML = `
                        <input type="number" 
                               id="${inputId}" 
                               class="form-control-filtro"
                               placeholder="Filtrar por ${columna.toLowerCase()}..."
                               data-tabla="${tabla}"
                               data-columna="${columna}" />
                    `;
                } else if (tipoCampo === 'select') {
                    const opciones = obtenerOpcionesSelect(tabla, columna);
                    inputHTML = `
                        <select id="${inputId}" 
                                class="form-control-filtro searchable-select"
                                data-tabla="${tabla}"
                                data-columna="${columna}">
                            <option value="">-- Todos --</option>
                            ${opciones.map(op => `<option value="${op}">${op}</option>`).join('')}
                        </select>
                    `;
                } else {
                    inputHTML = `
                        <input type="text" 
                               id="${inputId}" 
                               class="form-control-filtro"
                               placeholder="Filtrar por ${columna.toLowerCase()}..."
                               data-tabla="${tabla}"
                               data-columna="${columna}" />
                    `;
                }

                filtroDiv.innerHTML = `
                    <label for="${inputId}">
                        <i class="fa-solid fa-filter"></i> ${columna}:
                    </label>
                    ${inputHTML}
                `;

                filtrosGrid.appendChild(filtroDiv);

                const input = filtroDiv.querySelector('input, select');

                // CORRECCIÓN CRÍTICA: Usar 'change' para selects y 'input' para inputs
                const eventoEscucha = input.tagName === 'SELECT' ? 'change' : 'input';

                input.addEventListener(eventoEscucha, function () {
                    if (!filtrosSeleccionados[tabla]) {
                        filtrosSeleccionados[tabla] = {};
                    }

                    const valor = this.value.trim();
                    if (valor && valor !== '') {
                        filtrosSeleccionados[tabla][columna] = valor;
                        console.log(`Filtro agregado - Tabla: ${tabla}, Columna: ${columna}, Valor: "${valor}"`);
                    } else {
                        delete filtrosSeleccionados[tabla][columna];
                        console.log(`Filtro removido - Tabla: ${tabla}, Columna: ${columna}`);
                    }

                    console.log('📋 Filtros actuales:', JSON.stringify(filtrosSeleccionados, null, 2));
                });

                // Inicializar searchable select si es necesario
                if (tipoCampo === 'select') {
                    setTimeout(() => {
                        inicializarSearchableSelect(inputId);
                    }, 100);
                }
            });
        }
    });
}

function obtenerOpcionesSelect(tabla, columna) {
    if (tabla === 'productos' && columna === 'Categoría') {
        return datosSelects.categorias || [];
    }
    if (tabla === 'productos' && columna === 'Proveedores') {
        return datosSelects.proveedores || [];
    }
    if (tabla === 'actas' && columna === 'Proveedor') {
        return datosSelects.proveedores || [];
    }
    if (tabla === 'actas' && columna === 'Productos') {
        return datosSelects.productos || [];
    }
    if (tabla === 'movimientos' && columna === 'Producto') {
        return datosSelects.productos || [];
    }
    if (tabla === 'movimientos' && columna === 'Tipo') {
        return datosSelects.tiposMovimiento || [];
    }
    if (tabla === 'movimientos' && columna === 'Acta') {
        return datosSelects.actas || [];
    }
    return [];
}

function actualizarResumen() {
    const container = document.getElementById('resumenContenido');
    container.innerHTML = '';

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

async function generarReporte(guardarComo = false) {
    let nombrePersonalizado = null;

    if (guardarComo) {
        const { value: nombre } = await Swal.fire({
            title: 'Nombre del archivo',
            input: 'text',
            inputLabel: 'Ingrese el nombre para su reporte',
            inputPlaceholder: 'Ej: Reporte_Productos_2025',
            inputValue: `reporte_${new Date().toLocaleDateString('es-HN').replace(/\//g, '-')}`,
            showCancelButton: true,
            confirmButtonText: 'Guardar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            inputValidator: (value) => {
                if (!value) {
                    return 'Debe ingresar un nombre';
                }
                // Validar caracteres no permitidos en nombres de archivo
                if (/[<>:"/\\|?*]/.test(value)) {
                    return 'El nombre contiene caracteres no permitidos';
                }
            }
        });

        if (!nombre) return; // Usuario canceló
        nombrePersonalizado = nombre;
    }

    try {
        document.getElementById('loadingOverlay').style.display = 'flex';

        const payload = {
            tablas: tablasSeleccionadas,
            columnas: columnasSeleccionadas,
            filtros: filtrosSeleccionados,
            formato: formatoSeleccionado
        };

        console.log('Enviando payload:', JSON.stringify(payload, null, 2));

        const response = await fetch('/Reportes/Crear?handler=Generar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('Error del servidor:', errorText);
            throw new Error(`Error del servidor: ${response.status} - ${errorText}`);
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;

        const extension = formatoSeleccionado === 'excel' ? 'xlsx' :
            formatoSeleccionado === 'pdf' ? 'pdf' : 'docx';

        // Usar nombre personalizado o nombre por defecto
        if (nombrePersonalizado) {
            a.download = `${nombrePersonalizado}.${extension}`;
        } else {
            a.download = `reporte_${new Date().getTime()}.${extension}`;
        }

        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        document.getElementById('loadingOverlay').style.display = 'none';

        Swal.fire({
            icon: 'success',
            title: '¡Reporte generado!',
            text: 'Su reporte se ha descargado correctamente',
            confirmButtonText: 'Aceptar',
            confirmButtonColor: '#28a745'
        });

    } catch (error) {
        console.error('Error completo:', error);
        document.getElementById('loadingOverlay').style.display = 'none';

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Ocurrió un error al generar el reporte: ' + error.message,
            confirmButtonText: 'Aceptar',
            confirmButtonColor: '#dc3545'
        });
    }
}