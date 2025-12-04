document.addEventListener("DOMContentLoaded", function () {
    const successMessage = '@TempData["SuccessMessage"]';
    if (successMessage && successMessage !== 'null') {
        Swal.fire({
            icon: 'success',
            title: '¡Éxito!',
            text: successMessage,
            confirmButtonText: 'OK'
        });
    }