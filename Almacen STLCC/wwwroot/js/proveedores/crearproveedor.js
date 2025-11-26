document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const formCrearProveedor = document.getElementById('formCrearProveedor');
    let formModificado = false;

    if (formCrearProveedor) {
        formCrearProveedor.addEventListener('input', () => {
            formModificado = true;
        });

        formCrearProveedor.addEventListener('submit', () => {
            formModificado = false;
        });
    }

    function cerrarModal() {
        if (formModificado) {
            if (confirm('¿Estás seguro(a) de que deseas salir? Los cambios no guardados se perderán.')) {
                window.location.href = '/Proveedores/Index';
            }
        } else {
            window.location.href = '/Proveedores/Index';
        }
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