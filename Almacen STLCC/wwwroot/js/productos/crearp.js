    const modalOverlay = document.getElementById('modalOverlay');
    const btnClose = document.getElementById('btnClose');
    const btnCancelar = document.getElementById('btnCancelar');
    const formCrearProducto = document.getElementById('formCrearProducto');
    let formModificado = false;

    formCrearProducto.addEventListener('input', () => {
        formModificado = true;
    });

    function cerrarModal() {
        if (formModificado) {
            if (confirm('¿Estás seguro(a) de que deseas salir? Los cambios no guardados se perderán.')) {
        window.location.href = '/Productos/Index';
            }
        } else {
        window.location.href = '/Productos/Index';
        }
    }

    btnClose.addEventListener('click', cerrarModal);

    btnCancelar.addEventListener('click', cerrarModal);

    modalOverlay.addEventListener('click', (e) => {
        if (e.target === modalOverlay) {
        cerrarModal();
        }
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
        cerrarModal();
        }
    });

    formCrearProducto.addEventListener('submit', () => {
        formModificado = false;
    });