function mostrarFormulario() {
    document.getElementById('formularioSubida').style.display = 'flex';
}

function ocultarFormulario() {
    document.getElementById('formularioSubida').style.display = 'none';
    document.getElementById('inputArchivo').value = '';
    document.getElementById('archivosSeleccionados').innerHTML = '';
}

function mostrarArchivosSeleccionados(input) {
    const container = document.getElementById('archivosSeleccionados');
    container.innerHTML = '';

    if (input.files && input.files.length > 0) {
        Array.from(input.files).forEach(archivo => {
            const div = document.createElement('div');
            div.className = 'archivo-item';

            const tamañoKB = (archivo.size / 1024).toFixed(2);

            div.innerHTML = `
                <span class="nombre">${archivo.name}</span>
                <span class="tamaño">${tamañoKB} KB</span>
            `;

            container.appendChild(div);
        });
    }
}

function validarYSubir() {
    const input = document.getElementById('inputArchivo');
    const archivos = input.files;

    if (!archivos || archivos.length === 0) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Debe seleccionar al menos un archivo',
            confirmButtonText: 'Aceptar'
        });
        return;
    }

    const extensionesPermitidas = ['.jpg', '.jpeg', '.png', '.pdf', '.doc', '.docx'];
    const tamañoMaximo = 10 * 1024 * 1024;
    let errores = [];

    Array.from(archivos).forEach((archivo, index) => {
        const extension = '.' + archivo.name.split('.').pop().toLowerCase();

        if (!extensionesPermitidas.includes(extension)) {
            errores.push(`"${archivo.name}": formato no permitido`);
        }

        if (archivo.size > tamañoMaximo) {
            errores.push(`"${archivo.name}": supera los 10 MB`);
        }
    });

    if (errores.length > 0) {
        Swal.fire({
            icon: 'error',
            title: 'Archivo(s) inválido(s)',
            html: `<ul style="text-align: left;">${errores.map(e => `<li>${e}</li>`).join('')}</ul>`,
            confirmButtonText: 'Aceptar'
        });
        return;
    }

    Swal.fire({
        title: '¿Subir archivo(s)?',
        text: `Se subirá(n) ${archivos.length} archivo(s)`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, subir',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById('formArchivo').submit();
        }
    });
}

function confirmarEliminar(idAnexo, nombreArchivo) {
    Swal.fire({
        title: '¿Eliminar el archivo?',
        text: `Se eliminará "${nombreArchivo}"`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById('idAnexoEliminar').value = idAnexo;
            document.getElementById('formEliminar').submit();
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const dropzone = document.getElementById('dropzone');
    const inputArchivo = document.getElementById('inputArchivo');

    if (dropzone && inputArchivo) {
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropzone.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        ['dragenter', 'dragover'].forEach(eventName => {
            dropzone.addEventListener(eventName, () => {
                dropzone.classList.add('dragover');
            });
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropzone.addEventListener(eventName, () => {
                dropzone.classList.remove('dragover');
            });
        });

        dropzone.addEventListener('drop', (e) => {
            const files = e.dataTransfer.files;
            inputArchivo.files = files;
            mostrarArchivosSeleccionados(inputArchivo);
        });
    }
});
