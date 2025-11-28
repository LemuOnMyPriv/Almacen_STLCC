using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Usuarios
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int Id_Usuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(100)]
        [Column("nombreusuario")]
        public required string NombreUsuario { get; set; }

        [Required]
        [StringLength(255)]
        [Column("contraseña")]
        public required string Contraseña { get; set; }

        [Required]
        [Column("rol")]
        public string Rol { get; set; } = "USUARIO";
    }
}