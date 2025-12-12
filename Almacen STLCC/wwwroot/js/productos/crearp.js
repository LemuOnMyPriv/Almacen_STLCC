let formModificado = false;
let proveedoresSeleccionados = [];

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formCrearProducto = document.getElementById('formCrearProducto');

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

    configurarProveedoresDropdown();
});

function configurarProveedoresDropdown() {
    const btnAgregarProveedor = document.getElementById('btnAgregarProveedor');
    const dropdown = document.getElementById('dropdownProveedores');
    const searchInput = document.getElementById('searchProveedor');
    const listaProveedores = document.getElementById('listaProveedores');

    if (!btnAgregarProveedor || !dropdown) {
        console.error('No se encontraron elementos necesarios para el dropdown');
        return;
    }

    btnAgregarProveedor.addEventListener('click', (e) => {
        e.preventDefault();
        dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
        if (dropdown.style.display === 'block') {
            searchInput.focus();
            filtrarProveedores('');
        }
    });

    searchInput.addEventListener('input', (e) => {
        filtrarProveedores(e.target.value);
    });

    listaProveedores.addEventListener('click', (e) => {
        const item = e.target.closest('.proveedor-item');
        if (item) {
            const id = parseInt(item.dataset.id);
            const nombre = item.dataset.nombre;
            agregarProveedor(id, nombre);
            dropdown.style.display = 'none';
            searchInput.value = '';
            filtrarProveedores('');
        }
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('.proveedores-container') && !e.target.closest('#dropdownProveedores')) {
            dropdown.style.display = 'none';
        }
    });
}

function filtrarProveedores(texto) {
    const items = document.querySelectorAll('.proveedor-item');
    const textoLower = texto.toLowerCase();

    items.forEach(item => {
        const nombre = item.dataset.nombre.toLowerCase();
        const yaSeleccionado = proveedoresSeleccionados.some(p => p.id === parseInt(item.dataset.id));

        if (yaSeleccionado) {
            item.style.display = 'none';
        } else if (nombre.includes(textoLower)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

function agregarProveedor(id, nombre) {
    if (proveedoresSeleccionados.some(p => p.id === id)) {
        return;
    }

    proveedoresSeleccionados.push({ id, nombre });
    renderizarProveedores();
    actualizarProveedoresHidden();
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
            <button type="button" class="btn-eliminar-chip" data-proveedor-id="${proveedor.id}">
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

function eliminarProveedor(id) {
    proveedoresSeleccionados = proveedoresSeleccionados.filter(p => p.id !== id);
    renderizarProveedores();
    actualizarProveedoresHidden();
    formModificado = true;
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

    if (datos.proveedores && Array.isArray(datos.proveedores)) {
        proveedoresSeleccionados = datos.proveedores;
        renderizarProveedores();
        actualizarProveedoresHidden();
    }

    formModificado = true;
}

window.abrirCrearCategoria = function () {
    guardarDatosForm();
    abrirVentanaYRecargar('/Categorias/CrearCategoria');
};

window.abrirCrearProveedor = function () {
    guardarDatosForm();
    abrirVentanaYRecargar('/Proveedores/CrearProveedor');
};