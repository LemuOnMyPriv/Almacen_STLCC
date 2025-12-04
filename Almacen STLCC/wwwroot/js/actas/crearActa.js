let formModificado = false;
let productoIndex = 1;
let productosDisponibles = [];

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formCrearActa = document.getElementById('formCrearActa');
    const productosContainer = document.getElementById('productosContainer');

    const primerSelect = document.querySelector('select[name="Input.Detalles[0].Id_Producto"]');
    if (primerSelect) {
        productosDisponibles = Array.from(primerSelect.options).map(opt => ({
            value: opt.value,
            text: opt.text
        }));
    }

    if (formCrearActa) {
        formCrearActa.addEventListener('input', () => formModificado = true);
        formCrearActa.addEventListener('submit', () => formModificado = false);
    }

    function cerrarModal() {
        if (formModificado) {
            Swal.fire({
                title: '¿Salir sin guardar?',
                text: "Los cambios no guardados se perderán.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, salir',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '/Actas/Index';
                }
            });
        } else {
            window.location.href = '/Actas/Index';
        }
    }

    btnGuardar.addEventListener('click', function (e) {
        if (!formCrearActa) return;

        Swal.fire({
            title: '¿Estás seguro?',
            text: "Los datos serán guardados, asegúrate de que todo esté correcto",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sí, guardar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                formCrearActa.submit();
            }
        });
    });

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

    window.agregarProducto = function () {
        if (!productosContainer) return;

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
                    <button type="button" class="btn-eliminar-producto">Eliminar</button>
                </div>
            </div>
        `;

        productosContainer.appendChild(nuevoProducto);
        productoIndex++;
        actualizarBotonesEliminar();
    };

    function eliminarProductoHandler(e) {
        if (!e.target.classList.contains('btn-eliminar-producto')) return;
        const producto = e.target.closest('.producto-item');
        if (producto) {
            producto.remove();
            actualizarBotonesEliminar();
        }
    }

    productosContainer?.addEventListener('click', eliminarProductoHandler);

    function actualizarBotonesEliminar() {
        const productos = document.querySelectorAll('.producto-item');
        const botones = document.querySelectorAll('.btn-eliminar-producto');
        if (productos.length > 1) {
            botones.forEach(btn => btn.style.display = 'block');
        } else {
            botones.forEach(btn => btn.style.display = 'none');
        }
    }
});
