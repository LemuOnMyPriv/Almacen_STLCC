let formModificado = false;

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formRegistrarMovimiento = document.getElementById('formRegistrarMovimiento');

    // Inicializar searchable selects
    inicializarSearchableSelect('selectProducto');
    inicializarSearchableSelect('selectActa');

    if (formRegistrarMovimiento) {
        formRegistrarMovimiento.addEventListener('input', () => {
            formModificado = true;
        });
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