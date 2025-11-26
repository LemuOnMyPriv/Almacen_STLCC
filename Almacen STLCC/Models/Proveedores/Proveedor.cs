using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Proveedores
{
    [Table("proveedores")]
    public class Proveedor
    {
        [Key]
        [Column("id_proveedor")]
        public int Id_Proveedor { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(100)]
        [Column("nombre_proveedor")]
        public string Nombre_Proveedor { get; set; }

        [Required(ErrorMessage = "El RTN es obligatorio")]
        [StringLength(20)]
        [Column("rtn")]
        public string Rtn { get; set; }

        // Relaciones
        public ICollection<Productos.Producto> Productos { get; set; } = new List<Productos.Producto>();
        public ICollection<Actas.Acta> Actas { get; set; } = new List<Actas.Acta>();
    }
}