/**
 * Convierte un <select> normal en un dropdown con búsqueda
 * @param {string} selectId - ID del select a convertir
 * @param {Object} opciones - Configuración opcional
 */
function inicializarSearchableSelect(selectId, opciones = {}) {
    const select = document.getElementById(selectId);
    if (!select) {
        console.error(`No se encontró el select con ID: ${selectId}`);
        return;
    }

    // Configuración por defecto
    const config = {
        placeholder: opciones.placeholder || 'Buscar...',
        noResultsText: opciones.noResultsText || 'No se encontraron resultados',
        allowClear: opciones.allowClear !== false,
        ...opciones
    };

    // FUNCIÓN CLAVE: limpia texto proveniente de Razor / HTML
    function limpiarTexto(texto) {
        return texto
            .replace(/\s+/g, ' ') // elimina saltos de línea y espacios múltiples
            .trim();
    }

    // Crear el wrapper
    const wrapper = document.createElement('div');
    wrapper.className = 'searchable-select-wrapper';
    wrapper.style.position = 'relative';
    wrapper.style.width = '100%';

    // Ocultar el select original
    select.style.display = 'none';
    select.parentNode.insertBefore(wrapper, select);
    wrapper.appendChild(select);

    // Crear el input de búsqueda
    const searchInput = document.createElement('input');
    searchInput.type = 'text';
    searchInput.className = 'searchable-select-input form-control';
    searchInput.placeholder = config.placeholder;
    searchInput.autocomplete = 'off';
    searchInput.readOnly = true;

    // Ícono dropdown
    const dropdownIcon = document.createElement('span');
    dropdownIcon.className = 'searchable-select-icon';
    dropdownIcon.innerHTML = '<i class="fa-solid fa-chevron-down"></i>';

    // Contenedor input + ícono
    const inputContainer = document.createElement('div');
    inputContainer.className = 'searchable-select-input-container';
    inputContainer.appendChild(searchInput);
    inputContainer.appendChild(dropdownIcon);
    wrapper.appendChild(inputContainer);

    // Dropdown
    const dropdown = document.createElement('div');
    dropdown.className = 'searchable-select-dropdown';
    dropdown.style.display = 'none';
    wrapper.appendChild(dropdown);

    // Input filtro
    const filterInput = document.createElement('input');
    filterInput.type = 'text';
    filterInput.className = 'searchable-select-filter';
    filterInput.placeholder = 'Buscar...';
    filterInput.autocomplete = 'off';
    dropdown.appendChild(filterInput);

    // Lista opciones
    const optionsList = document.createElement('div');
    optionsList.className = 'searchable-select-options';
    dropdown.appendChild(optionsList);

    // Render opciones
    function renderOptions(filtro = '') {
        optionsList.innerHTML = '';
        const options = Array.from(select.options);
        let hayResultados = false;

        options.forEach((option, index) => {
            if (index === 0 && option.value === '') return;

            const textoLimpio = limpiarTexto(option.textContent);
            const textoBusqueda = textoLimpio.toLowerCase();

            if (filtro === '' || textoBusqueda.includes(filtro.toLowerCase())) {
                hayResultados = true;

                const optionDiv = document.createElement('div');
                optionDiv.className = 'searchable-select-option';
                if (option.selected) optionDiv.classList.add('selected');

                optionDiv.textContent = textoLimpio;
                optionDiv.dataset.value = option.value;

                optionDiv.addEventListener('click', function () {
                    select.value = option.value;
                    select.dispatchEvent(new Event('change', { bubbles: true }));
                    searchInput.value = textoLimpio;
                    cerrarDropdown();
                });

                optionsList.appendChild(optionDiv);
            }
        });

        if (!hayResultados) {
            const noResults = document.createElement('div');
            noResults.className = 'searchable-select-no-results';
            noResults.textContent = config.noResultsText;
            optionsList.appendChild(noResults);
        }
    }

    function abrirDropdown() {
        dropdown.style.display = 'block';
        filterInput.value = '';
        filterInput.focus();
        renderOptions();
        dropdownIcon.innerHTML = '<i class="fa-solid fa-chevron-up"></i>';
    }

    function cerrarDropdown() {
        dropdown.style.display = 'none';
        dropdownIcon.innerHTML = '<i class="fa-solid fa-chevron-down"></i>';
    }

    // Eventos
    inputContainer.addEventListener('click', function (e) {
        e.stopPropagation();
        dropdown.style.display === 'none' ? abrirDropdown() : cerrarDropdown();
    });

    filterInput.addEventListener('input', function () {
        renderOptions(this.value);
    });

    filterInput.addEventListener('click', e => e.stopPropagation());

    document.addEventListener('click', function (e) {
        if (!wrapper.contains(e.target)) cerrarDropdown();
    });

    // Cambio programático
    select.addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        searchInput.value = selectedOption && selectedOption.value !== ''
            ? limpiarTexto(selectedOption.textContent)
            : '';
    });

    // Valor inicial
    const selectedOption = select.options[select.selectedIndex];
    if (selectedOption && selectedOption.value !== '') {
        searchInput.value = limpiarTexto(selectedOption.textContent);
    }

    // Escape
    filterInput.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') cerrarDropdown();
    });
}

/**
 * Inicializa todos los selects con la clase 'searchable-select'
 */
function inicializarTodosLosSearchableSelects() {
    document.querySelectorAll('select.searchable-select').forEach(select => {
        if (select.id) inicializarSearchableSelect(select.id);
    });
}
    