let formModificado = false;
let productoIndex = 1;
let productosDisponibles = [];

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formCrearActa = document.getElementById('formCrearActa');

    const primerSelect = document.querySelector('select[name="Input.Detalles[0].Id_Producto"]');
    if (primerSelect) {
        productosDisponibles = Array.from(primerSelect.options).map(opt => ({
            value: opt.value,
            text: opt.text
        }));
    }

    // Inicializar searchable selects
    inicializarSearchableSelect('selectProveedor');
    inicializarSearchableSelect('selectProducto0');

    cargarDatosGuardados();

    if (formCrearActa) {
        formCrearActa.addEventListener('input', () => {
            formModificado = true;
        });
    }

    if (btnGuardar) {
        btnGuardar.addEventListener('click', function (e) {
            e.preventDefault();
            confirmarGuardado(formCrearActa, '¿Está seguro de guardar esta acta?');
        });
    }

    function cerrarModal() {
        cerrarModalConConfirmacion('formCrearActa', '/Actas/Index', formModificado);
    }

    if (btnCancelar) btnCancelar.addEventListener('click', cerrarModal);
    if (btnClose) btnClose.addEventListener('click', cerrarModal);
    if (modalOverlay) {
        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) cerrarModal();
        });
    }

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') cerrarModal();
    });

    configurarBotonesCrear();
});

function configurarBotonesCrear() {
    const btnCrearProveedor = document.getElementById('btnCrearProveedorActa');
    const btnCrearProducto = document.getElementById('btnCrearProductoActa');

    if (btnCrearProveedor) {
        btnCrearProveedor.addEventListener('click', () => {
            guardarDatosForm();
            abrirVentanaYRecargar('/Proveedores/CrearProveedor', 600, 500);
        });
    }

    if (btnCrearProducto) {
        btnCrearProducto.addEventListener('click', () => {
            guardarDatosForm();
            abrirVentanaYRecargar('/Productos/CrearP', 800, 600);
        });
    }
}

function agregarProducto() {
    const container = document.getElementById('productosContainer');

    const nuevoProducto = document.createElement('div');
    nuevoProducto.className = 'producto-item';
    nuevoProducto.setAttribute('data-index', productoIndex);

    let optionsHTML = '<option value="">-- Seleccione un producto --</option>';
    productosDisponibles.forEach(prod => {
        if (prod.value) {
            optionsHTML += `<option value="${prod.value}">${prod.text}</option>`;
        }
    });

    nuevoProducto.innerHTML = `
        <div class="form-grid">
            <div class="form-group">
                <label>Producto:</label>
                <select name="Input.Detalles[${productoIndex}].Id_Producto" 
                        class="form-select searchable-select" 
                        id="selectProducto${productoIndex}"
                        required>
                    ${optionsHTML}
                </select>
            </div>

            <div class="form-group">
                <label>Requisición (Opcional):</label>
                <input type="text" 
                       name="Input.Detalles[${productoIndex}].Requisicion" 
                       class="form-control" 
                       placeholder="Ej: REQ-2025-001"
                       autocomplete="off" />
                <small class="form-text">Puede dejarlo vacío si no aplica</small>
            </div>

            <div class="form-group">
                <label>Cantidad:</label>
                <input type="number" 
                       name="Input.Detalles[${productoIndex}].Cantidad" 
                       class="form-control" 
                       min="1" 
                       placeholder="Ej: 10"
                       required />
            </div>

            <div class="form-group">
                <label>Precio Unitario:</label>
                <input type="number" 
                       name="Input.Detalles[${productoIndex}].Precio_Unitario" 
                       class="form-control" 
                       step="0.01" 
                       min="0"
                       placeholder="Sin ISV" />
            </div>

            <div class="form-group">
                <label>Precio con ISV:</label>
                <input type="number" 
                       name="Input.Detalles[${productoIndex}].Precio_Con_Isv" 
                       class="form-control" 
                       step="0.01" 
                       min="0"
                       placeholder="Con ISV incluido" />
            </div>

            <div class="form-group btn-eliminar-container">
                <button type="button" class="btn-eliminar-producto" data-producto-index="${productoIndex}">Eliminar</button>
            </div>
        </div>
    `;

    const btnEliminar = nuevoProducto.querySelector('.btn-eliminar-producto');
    const index = productoIndex;
    btnEliminar.addEventListener('click', () => {
        eliminarProducto(index);
    });

    container.appendChild(nuevoProducto);

    // Inicializar searchable select para el nuevo producto
    inicializarSearchableSelect(`selectProducto${productoIndex}`);

    productoIndex++;
    actualizarBotonesEliminar();
}

function eliminarProducto(index) {
    const producto = document.querySelector(`.producto-item[data-index="${index}"]`);
    if (producto) {
        producto.remove();
        reindexarProductos(); // REINDEXAR después de eliminar
        actualizarBotonesEliminar();
    }
}

// Nueva función para reindexar productos
function reindexarProductos() {
    const productos = document.querySelectorAll('.producto-item');
    productos.forEach((item, nuevoIndex) => {
        // Actualizar data-index
        item.setAttribute('data-index', nuevoIndex);

        // Actualizar todos los name de inputs dentro del producto
        const inputs = item.querySelectorAll('input, select');
        inputs.forEach(input => {
            const name = input.getAttribute('name');
            if (name) {
                // Reemplazar el índice en el name
                const nuevoName = name.replace(/\[\d+\]/, `[${nuevoIndex}]`);
                input.setAttribute('name', nuevoName);
            }
        });

        // Actualizar el ID del select si es searchable
        const select = item.querySelector('select');
        if (select && select.id) {
            const nuevoId = `selectProducto${nuevoIndex}`;

            // Si ya estaba inicializado como searchable, destruir y recrear
            const wrapper = select.closest('.searchable-select-wrapper');
            if (wrapper) {
                // Destruir el searchable select
                const valorActual = select.value;
                const inputContainer = wrapper.querySelector('.searchable-select-input-container');
                const dropdown = wrapper.querySelector('.searchable-select-dropdown');
                if (inputContainer) inputContainer.remove();
                if (dropdown) dropdown.remove();
                select.style.display = '';
                wrapper.replaceWith(select);

                // Actualizar ID y recrear
                select.id = nuevoId;
                inicializarSearchableSelect(nuevoId);
                select.value = valorActual;
            } else {
                select.id = nuevoId;
            }
        }

        // Actualizar el botón de eliminar
        const btnEliminar = item.querySelector('.btn-eliminar-producto');
        if (btnEliminar) {
            btnEliminar.setAttribute('data-producto-index', nuevoIndex);
            btnEliminar.onclick = () => eliminarProducto(nuevoIndex);
        }
    });

    // Actualizar el índice global para el siguiente producto
    productoIndex = productos.length;
}

function actualizarBotonesEliminar() {
    const productos = document.querySelectorAll('.producto-item');
    const botones = document.querySelectorAll('.btn-eliminar-producto');

    if (productos.length > 1) {
        botones.forEach(btn => btn.style.display = 'block');
    } else {
        botones.forEach(btn => btn.style.display = 'none');
    }
}

function guardarDatosForm() {
    const productos = [];
    document.querySelectorAll('.producto-item').forEach(item => {
        const selects = item.querySelectorAll('select');
        const inputs = item.querySelectorAll('input[type="number"], input[type="text"]');

        if (selects.length > 0) {
            productos.push({
                idProducto: selects[0].value,
                requisicion: inputs[0]?.value || '',
                cantidad: inputs[1]?.value || '',
                precioUnitario: inputs[2]?.value || '',
                precioConIsv: inputs[3]?.value || ''
            });
        }
    });

    const datos = {
        f01: document.querySelector('[name="Input.F01"]')?.value || '',
        ordenCompra: document.querySelector('[name="Input.Orden_Compra"]')?.value || '',
        numeroActa: document.querySelector('[name="Input.Numero_Acta"]')?.value || '',
        proveedor: document.querySelector('#selectProveedor')?.value || '',
        fecha: document.querySelector('[name="Input.Fecha"]')?.value || '',
        productos: productos
    };

    guardarDatosFormulario('formActa', datos);
}

function cargarDatosGuardados() {
    const datos = recuperarDatosFormulario('formActa');
    if (!datos) return;

    if (datos.numeroActa) document.querySelector('[name="Input.Numero_Acta"]').value = datos.numeroActa;
    if (datos.f01) document.querySelector('[name="Input.F01"]').value = datos.f01;
    if (datos.ordenCompra) document.querySelector('[name="Input.Orden_Compra"]').value = datos.ordenCompra;
    if (datos.proveedor) document.querySelector('#selectProveedor').value = datos.proveedor;
    if (datos.fecha) document.querySelector('[name="Input.Fecha"]').value = datos.fecha;

    if (datos.productos && Array.isArray(datos.productos) && datos.productos.length > 0) {
        datos.productos.forEach((prod, idx) => {
            if (idx > 0) agregarProducto();

            const items = document.querySelectorAll('.producto-item');
            const item = items[idx];

            if (item) {
                const select = item.querySelector('select');
                const inputs = item.querySelectorAll('input');

                if (select && prod.idProducto) select.value = prod.idProducto;
                if (inputs[0] && prod.requisicion) inputs[0].value = prod.requisicion;
                if (inputs[1] && prod.cantidad) inputs[1].value = prod.cantidad;
                if (inputs[2] && prod.precioUnitario) inputs[2].value = prod.precioUnitario;
                if (inputs[3] && prod.precioConIsv) inputs[3].value = prod.precioConIsv;
            }
        });
    }

    formModificado = true;
}