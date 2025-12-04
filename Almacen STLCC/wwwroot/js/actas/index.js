document.addEventListener("DOMContentLoaded", function () {
    const messageDiv = document.getElementById('successMessage');
    if (messageDiv) {
        const message = JSON.parse(messageDiv.dataset.message);
        if (message) {
            Swal.fire({
                icon: 'success',
                title: '¡Éxito!',
                text: message,
                confirmButtonText: 'Aceptar'
            });
        }
    }
});