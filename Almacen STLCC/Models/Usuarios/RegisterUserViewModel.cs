using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Models.Usuarios
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Ingrese un nombre de usuario")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no debe superar los 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "Ingrese una contraseña")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public required string Contraseña { get; set; }

        [Required(ErrorMessage = "Es necesario confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public required string ConfirmarContraseña { get; set; }
    }
}
