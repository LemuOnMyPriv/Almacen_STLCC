using Almacen_STLCC.Models.Productos;
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
        public required string Nombre_Proveedor { get; set; }

        [Required(ErrorMessage = "El RTN es obligatorio")]
        [StringLength(20)]
        [Column("rtn")]
        public required string Rtn { get; set; }

        // Relaciones

        public ICollection<Productos.ProductoProveedor> ProductoProveedores { get; set; } = [];
        public ICollection<Actas.Acta> Actas { get; set; } = [];
    }
}