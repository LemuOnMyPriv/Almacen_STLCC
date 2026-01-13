function abrirModalRequisicion(idDetalle, nombreProducto, requisicionActual) {
    const modal = document.getElementById('modalRequisicion');
    const tituloModal = document.getElementById('tituloModal');
    const nombreProductoSpan = document.getElementById('nombreProducto');
    const idDetalleInput = document.getElementById('idDetalle');
    const requisicionInput = document.getElementById('requisicion');

    // Configurar el modal
    nombreProductoSpan.textContent = nombreProducto;
    idDetalleInput.value = idDetalle;
    requisicionInput.value = requisicionActual;

    // Cambiar título según si hay requisición o no
    if (requisicionActual) {
        tituloModal.textContent = 'Editar Requisición';
    } else {
        tituloModal.textContent = 'Agregar Requisición';
    }

    // Mostrar el modal
    modal.style.display = 'flex';

    // Focus en el input
    setTimeout(() => requisicionInput.focus(), 100);
}

function cerrarModalRequisicion() {
    const modal = document.getElementById('modalRequisicion');
    modal.style.display = 'none';

    // Limpiar el formulario
    document.getElementById('formRequisicion').reset();
}

// Cerrar modal con tecla Escape
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('modalRequisicion');
        if (modal && modal.style.display === 'flex') {
            cerrarModalRequisicion();
        }
    }
});

// Cerrar modal al hacer clic fuera de él
document.getElementById('modalRequisicion')?.addEventListener('click', function (e) {
    if (e.target === this) {
        cerrarModalRequisicion();
    }
});