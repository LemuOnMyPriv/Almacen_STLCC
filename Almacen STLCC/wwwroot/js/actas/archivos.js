function mostrarFormulario() {
    document.getElementById('formularioSubida').style.display = 'block';
}

function ocultarFormulario() {
    document.getElementById('formularioSubida').style.display = 'none';
    document.getElementById('inputArchivo').value = '';
    document.getElementById('archivoSeleccionado').textContent = '';
}

function mostrarNombreArchivo(input) {
    const archivo = input.files[0];
    if (archivo) {
        document.getElementById('archivoSeleccionado').textContent = ` ${archivo.name}`;
    }
}

const dropzone = document.getElementById('dropzone');
const inputArchivo = document.getElementById('inputArchivo');

if (dropzone) {
    dropzone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropzone.classList.add('dragover');
    });

    dropzone.addEventListener('dragleave', () => {
        dropzone.classList.remove('dragover');
    });

    dropzone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropzone.classList.remove('dragover');

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            inputArchivo.files = files;
            mostrarNombreArchivo(inputArchivo);
        }
    });
}