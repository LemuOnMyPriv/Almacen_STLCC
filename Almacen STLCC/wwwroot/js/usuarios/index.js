document.addEventListener("DOMContentLoaded", function () {
    // Inicializar filtro múltiple de tabla
    inicializarFiltroMultiple({
        'filtroNombreUsuario': 0,  // Columna 0: Nombre de usuario
        'filtroRol': 1              // Columna 1: Rol
    }, 'tablaUsuarios');
});

inicializarPaginacion('tablaUsuarios', 25);