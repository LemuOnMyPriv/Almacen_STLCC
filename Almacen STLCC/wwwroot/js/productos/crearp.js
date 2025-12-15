let formModificado = false;
let proveedoresSeleccionados = [];

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formCrearProducto = document.getElementById('formCrearProducto');
    const selectProveedorInicial = document.getElementById('selectProveedorInicial');
    const btnAgregarOtroProveedor = document.getElementById('btnAgregarOtroProveedor');
    const btnCrearCategoria = document.getElementById('btnCrearCategoria');
    const btnCrearProveedor = document.getElementById('btnCrearProveedor');

    cargarDatosGuardados();

    if (formCrearProducto) {
        formCrearProducto.addEventListener('input', () => {
            formModificado = true;
        });
    }

    if (btnGuardar) {
        btnGuardar.addEventListener('click', function (e) {
            e.preventDefault();
            actualizarProveedoresHidden();
            confirmarGuardado(formCrearProducto, '¿Está seguro de guardar este producto?');
        });
    }

    function cerrarModal() {
        cerrarModalConConfirmacion('formCrearProducto', '/Productos/Index', formModificado);
    }

    if (btnClose) btnClose.addEventListener('click', cerrarModal);
    if (btnCancelar) btnCancelar.addEventListener('click', cerrarModal);
    if (modalOverlay) {
        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) cerrarModal();
        });
    }

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') cerrarModal();
    });

    if (selectProveedorInicial) {
        selectProveedorInicial.addEventListener('change', function () {
            const proveedorId = parseInt(this.value);
            const proveedorNombre = this.options[this.selectedIndex].dataset.nombre;

            if (proveedorId && proveedorNombre) {
                agregarProveedor(proveedorId, proveedorNombre);

                this.style.display = 'none';

                document.getElementById('proveedoresChipsContainer').style.display = 'block';
            }
        });
    }

    if (btnAgregarOtroProveedor) {
        btnAgregarOtroProveedor.addEventListener('click', function () {
            mostrarSelectorProveedores();
        });
    }

    if (btnCrearCategoria) {
        btnCrearCategoria.addEventListener('click', function () {
            guardarDatosForm();
            abrirVentanaYRecargar('/Categorias/CrearCategoria', 600, 500);
        });
    }

    if (btnCrearProveedor) {
        btnCrearProveedor.addEventListener('click', function () {
            guardarDatosForm();
            abrirVentanaYRecargar('/Proveedores/CrearProveedor', 600, 500);
        });
    }
});

function agregarProveedor(id, nombre) {
    if (proveedoresSeleccionados.some(p => p.id === id)) {
        return;
    }

    proveedoresSeleccionados.push({ id, nombre });
    renderizarProveedores();
    actualizarProveedoresHidden();
    formModificado = true;
}

function eliminarProveedor(id) {
    proveedoresSeleccionados = proveedoresSeleccionados.filter(p => p.id !== id);
    renderizarProveedores();
    actualizarProveedoresHidden();

    if (proveedoresSeleccionados.length === 0) {
        document.getElementById('selectProveedorInicial').style.display = 'block';
        document.getElementById('selectProveedorInicial').value = '';
        document.getElementById('proveedoresChipsContainer').style.display = 'none';
    }

    formModificado = true;
}

function renderizarProveedores() {
    const container = document.getElementById('proveedoresChips');
    if (!container) return;

    container.innerHTML = '';

    proveedoresSeleccionados.forEach(proveedor => {
        const chip = document.createElement('div');
        chip.className = 'proveedor-chip';
        chip.innerHTML = `
            <span>${proveedor.nombre}</span>
            <button type="button" class="btn-eliminar-chip">
                <i class="fa-solid fa-times"></i>
            </button>
        `;

        const btnEliminar = chip.querySelector('.btn-eliminar-chip');
        btnEliminar.addEventListener('click', () => {
            eliminarProveedor(proveedor.id);
        });

        container.appendChild(chip);
    });
}

function mostrarSelectorProveedores() {
    const proveedoresDisponibles = window.proveedoresData.filter(
        p => !proveedoresSeleccionados.some(ps => ps.id === p.id)
    );

    if (proveedoresDisponibles.length === 0) {
        alert('No hay más proveedores disponibles');
        return;
    }

    const modal = document.createElement('div');
    modal.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0,0,0,0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
    `;

    const contenido = document.createElement('div');
    contenido.style.cssText = `
        background: white;
        padding: 20px;
        border-radius: 8px;
        max-width: 400px;
        width: 90%;
    `;

    let selectHTML = `
        <h3 style="margin-top: 0;">Seleccionar Proveedor</h3>
        <select id="selectorTemporal" class="form-select" style="width: 100%; padding: 10px; margin-bottom: 15px;">
            <option value="">-- Seleccione --</option>
    `;

    proveedoresDisponibles.forEach(p => {
        selectHTML += `<option value="${p.id}" data-nombre="${p.nombre}">${p.nombre}</option>`;
    });

    selectHTML += `
        </select>
        <div style="display: flex; gap: 10px;">
            <button id="btnAceptarTemp" class="btn-guardar" style="flex: 1;">Agregar</button>
            <button id="btnCancelarTemp" class="btn-cancelar" style="flex: 1;">Cancelar</button>
        </div>
    `;

    contenido.innerHTML = selectHTML;
    modal.appendChild(contenido);
    document.body.appendChild(modal);

    // Event listeners
    document.getElementById('btnAceptarTemp').addEventListener('click', () => {
        const select = document.getElementById('selectorTemporal');
        const proveedorId = parseInt(select.value);
        const proveedorNombre = select.options[select.selectedIndex].dataset.nombre;

        if (proveedorId && proveedorNombre) {
            agregarProveedor(proveedorId, proveedorNombre);
        }

        document.body.removeChild(modal);
    });

    document.getElementById('btnCancelarTemp').addEventListener('click', () => {
        document.body.removeChild(modal);
    });
}

function actualizarProveedoresHidden() {
    const hidden = document.getElementById('hiddenProveedores');
    if (hidden) {
        hidden.value = proveedoresSeleccionados.map(p => p.id).join(',');
    }
}

function guardarDatosForm() {
    const datos = {
        codigo: document.querySelector('[name="Input.Codigo_Producto"]')?.value || '',
        nombre: document.querySelector('[name="Input.Nombre_Producto"]')?.value || '',
        marca: document.querySelector('[name="Input.Marca"]')?.value || '',
        categoria: document.querySelector('#selectCategoria')?.value || '',
        unidad: document.querySelector('[name="Input.Unidad_Medida"]')?.value || '',
        proveedores: proveedoresSeleccionados
    };
    guardarDatosFormulario('formProducto', datos);
}

function cargarDatosGuardados() {
    const datos = recuperarDatosFormulario('formProducto');
    if (!datos) return;

    if (datos.codigo) document.querySelector('[name="Input.Codigo_Producto"]').value = datos.codigo;
    if (datos.nombre) document.querySelector('[name="Input.Nombre_Producto"]').value = datos.nombre;
    if (datos.marca) document.querySelector('[name="Input.Marca"]').value = datos.marca;
    if (datos.categoria) document.querySelector('#selectCategoria').value = datos.categoria;
    if (datos.unidad) document.querySelector('[name="Input.Unidad_Medida"]').value = datos.unidad;

    if (datos.proveedores && Array.isArray(datos.proveedores) && datos.proveedores.length > 0) {
        proveedoresSeleccionados = datos.proveedores;

        document.getElementById('selectProveedorInicial').style.display = 'none';
        document.getElementById('proveedoresChipsContainer').style.display = 'block';

        renderizarProveedores();
        actualizarProveedoresHidden();
    }

    formModificado = true;
}