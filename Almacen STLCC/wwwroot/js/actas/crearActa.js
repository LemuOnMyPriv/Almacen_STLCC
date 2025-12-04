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
        <div class="form-grid">
            <div class="form-group">
                <label>Producto:</label>
                <select name="Input.Detalles[${productoIndex}].Id_Producto" class="form-select" required>
                    ${optionsHTML}
                </select>
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
                <button type="button" class="btn-eliminar-producto" onclick="eliminarProducto(${productoIndex})">Eliminar</button>
            </div>
        </div>
    `;

    container.appendChild(nuevoProducto);
    productoIndex++;
    actualizarBotonesEliminar();
}

function eliminarProducto(index) {
    const producto = document.querySelector(`.producto-item[data-index="${index}"]`);
    if (producto) {
        producto.remove();
        actualizarBotonesEliminar();
    }
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