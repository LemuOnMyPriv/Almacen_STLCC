using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Models.Usuarios
{
    public class RegisterUserViewModel
    {
        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContraseña { get; set; }
    }
}