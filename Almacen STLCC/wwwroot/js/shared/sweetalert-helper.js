function confirmarGuardado(form, mensaje = '¿Está seguro de guardar los cambios?') {
    Swal.fire({
        title: '¿Confirmar acción?',
        text: mensaje,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, guardar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            form.submit();
        }
    });
}

function mostrarMensajeExito() {
    const messageDiv = document.getElementById('successMessage');
    if (messageDiv) {
        const message = messageDiv.dataset.message;
        if (message && message !== '' && message !== 'null') {
            Swal.fire({
                icon: 'success',
                title: '¡Éxito!',
                text: message,
                confirmButtonText: 'Aceptar'
            });
        }
    }
}

function cerrarModalConConfirmacion(formId, redirectUrl, formModificado) {
    if (formModificado) {
        Swal.fire({
            title: '¿Salir sin guardar?',
            text: 'Los cambios no guardados se perderán',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, salir',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = redirectUrl;
            }
        });
    } else {
        window.location.href = redirectUrl;
    }
}

function guardarDatosFormulario(clave, datos) {
    try {
        sessionStorage.setItem(clave, JSON.stringify(datos));
    } catch (error) {
        console.error('Error al guardar datos en sessionStorage:', error);
    }
}
 
function recuperarDatosFormulario(clave) {
    try {
        const datos = sessionStorage.getItem(clave);
        if (datos) {
            sessionStorage.removeItem(clave);
            return JSON.parse(datos);
        }
    } catch (error) {
        console.error('Error al recuperar datos de sessionStorage:', error);
    }
    return null;
}

function abrirVentanaYRecargar(url, ancho = 1000, alto = 800) {
    window.open(url, '_blank');
    window.addEventListener('focus', () => {
        setTimeout(() => location.reload(), 500);
    }, { once: true });
}