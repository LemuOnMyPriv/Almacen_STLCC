let formModificado = false;
let productoIndex = productoIndexInicial; // Viene del cshtml

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formEditarActa = document.getElementById('formEditarActa');

    // Inicializar searchable selects existentes
    inicializarSearchableSelect('selectProveedor');

    // Inicializar searchable select para cada producto existente
    for (let i = 0; i < productoIndexInicial; i++) {
        inicializarSearchableSelect(`selectProducto${i}`);
    }

    if (formEditarActa) {
        formEditarActa.addEventListener('input', () => {
            formModificado = true;
        });
    }

    if (btnGuardar) {
        btnGuardar.addEventListener('click', function (e) {
            e.preventDefault();
            confirmarGuardado(formEditarActa, '¿Está seguro de guardar los cambios?');
        });
    }

    function cerrarModal() {
        cerrarModalConConfirmacion('formEditarActa', '/Actas/Index', formModificado);
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

    actualizarBotonesEliminar();
});

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
        <input type="hidden" name="Input.Detalles[${productoIndex}].Id_Detalle" value="0" />
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
        reindexarProductos();
        actualizarBotonesEliminar();
    }
}

function reindexarProductos() {
    const productos = document.querySelectorAll('.producto-item');
    productos.forEach((item, nuevoIndex) => {
        item.setAttribute('data-index', nuevoIndex);

        const inputs = item.querySelectorAll('input, select');
        inputs.forEach(input => {
            const name = input.getAttribute('name');
            if (name) {
                const nuevoName = name.replace(/\[\d+\]/, `[${nuevoIndex}]`);
                input.setAttribute('name', nuevoName);
            }
        });

        const select = item.querySelector('select');
        if (select && select.id) {
            const nuevoId = `selectProducto${nuevoIndex}`;
            const wrapper = select.closest('.searchable-select-wrapper');
            if (wrapper) {
                const valorActual = select.value;
                const inputContainer = wrapper.querySelector('.searchable-select-input-container');
                const dropdown = wrapper.querySelector('.searchable-select-dropdown');
                if (inputContainer) inputContainer.remove();
                if (dropdown) dropdown.remove();
                select.style.display = '';
                wrapper.replaceWith(select);

                select.id = nuevoId;
                inicializarSearchableSelect(nuevoId);
                select.value = valorActual;
            } else {
                select.id = nuevoId;
            }
        }

        const btnEliminar = item.querySelector('.btn-eliminar-producto');
        if (btnEliminar) {
            btnEliminar.setAttribute('data-producto-index', nuevoIndex);
            btnEliminar.onclick = () => eliminarProducto(nuevoIndex);
        }
    });

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