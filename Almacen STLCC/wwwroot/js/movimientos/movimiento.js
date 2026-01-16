let formModificado = false;

// Cache de inventarios por producto
const inventarioProductos = {};

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formRegistrarMovimiento = document.getElementById('formRegistrarMovimiento');
    const selectProducto = document.getElementById('selectProducto');
    const selectTipoMovimiento = document.getElementById('selectTipoMovimiento');
    const hintInventario = document.getElementById('hintInventario');
    const textoInventario = document.getElementById('textoInventario');
    const labelCantidad = document.getElementById('labelCantidad');
    const inputCantidad = document.getElementById('inputCantidad');

    // Inicializar searchable selects
    inicializarSearchableSelect('selectProducto');
    inicializarSearchableSelect('selectActa');

    if (formRegistrarMovimiento) {
        formRegistrarMovimiento.addEventListener('input', () => {
            formModificado = true;
        });
    }

    // Manejar cambio de tipo de movimiento
    if (selectTipoMovimiento) {
        selectTipoMovimiento.addEventListener('change', async function () {
            const tipo = this.value;
            const productoId = selectProducto.value;

            if (tipo === 'ajuste') {
                labelCantidad.textContent = 'Nueva Cantidad (Ajuste):';
                inputCantidad.placeholder = 'Cantidad final que debe quedar';

                if (productoId) {
                    await mostrarInventarioActual(productoId);
                } else {
                    textoInventario.textContent = 'Seleccione primero un producto';
                    hintInventario.style.display = 'block';
                }
            } else {
                hintInventario.style.display = 'none';
                labelCantidad.textContent = 'Cantidad:';
                inputCantidad.placeholder = 'Ej: 10';
            }
        });
    }

    // Manejar cambio de producto
    if (selectProducto) {
        selectProducto.addEventListener('change', async function () {
            const tipo = selectTipoMovimiento.value;
            const productoId = this.value;

            if (tipo === 'ajuste' && productoId) {
                await mostrarInventarioActual(productoId);
            }
        });
    }

    // Función para obtener y mostrar inventario actual
    async function mostrarInventarioActual(productoId) {
        try {
            // Si ya tenemos el dato en cache, usarlo
            if (inventarioProductos[productoId] !== undefined) {
                mostrarHint(inventarioProductos[productoId]);
                return;
            }

            // Mostrar mensaje de carga
            textoInventario.textContent = 'Cargando inventario...';
            hintInventario.style.display = 'block';

            // Llamar al handler OnGetInventario de la Razor Page
            const response = await fetch(`/Movimientos/Movimiento?handler=Inventario&id=${productoId}`);

            if (!response.ok) {
                throw new Error('Error al obtener inventario');
            }

            const data = await response.json();

            // Guardar en cache
            inventarioProductos[productoId] = data.cantidad;

            // Mostrar hint
            mostrarHint(data.cantidad);

        } catch (error) {
            console.error('Error al obtener inventario:', error);
            textoInventario.textContent = 'Error al cargar inventario. Ingrese la cantidad deseada.';
            hintInventario.style.display = 'block';
        }
    }

    function mostrarHint(cantidad) {
        textoInventario.textContent = `Inventario actual: ${cantidad} unidades`;
        hintInventario.style.display = 'block';
    }

    if (btnGuardar) {
        btnGuardar.addEventListener('click', function (e) {
            e.preventDefault();
            confirmarGuardado(formRegistrarMovimiento, '¿Está seguro de registrar este movimiento?');
        });
    }

    function cerrarModal() {
        cerrarModalConConfirmacion('formRegistrarMovimiento', '/Movimientos/Index', formModificado);
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
});