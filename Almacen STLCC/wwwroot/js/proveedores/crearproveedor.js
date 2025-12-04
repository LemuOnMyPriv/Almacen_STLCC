let formModificado = false;

document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const btnGuardar = document.getElementById('btnGuardar');
    const formCrearProveedor = document.getElementById('formCrearProveedor');

    if (formCrearProveedor) {
        formCrearProveedor.addEventListener('input', () => {
            formModificado = true;
        });
    }

if (btnGuardar) {
        btnGuardar.addEventListener('click', function (e) {
            e.preventDefault();
            confirmarGuardado(formCrearProveedor, '¿Está seguro de guardar este proveedor?');
        });
    }

    function cerrarModal() {
        cerrarModalConConfirmacion('formCrearProveedor', '/Proveedores/Index', formModificado);
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