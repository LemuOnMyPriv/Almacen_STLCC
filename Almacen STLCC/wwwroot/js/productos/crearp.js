document.addEventListener("DOMContentLoaded", function () {
    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const formCrearProducto = document.getElementById('formCrearProducto');
    let formModificado = false;

    if (formCrearProducto) {
        formCrearProducto.addEventListener('input', () => {
            formModificado = true;
        });

        formCrearProducto.addEventListener('submit', () => {
            formModificado = false;
        });
    }

    function cerrarModal() {
        if (formModificado) {
            if (confirm('¿Estás seguro(a) de que deseas salir? Los cambios no guardados se perderán.')) {
                window.location.href = '/Productos/Index';
            }
        } else {
            window.location.href = '/Productos/Index';
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