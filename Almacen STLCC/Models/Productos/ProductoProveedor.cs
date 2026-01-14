using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Almacen_STLCC.Models.Proveedores;

namespace Almacen_STLCC.Models.Productos
{
    [Table("producto_proveedor")]
    public class ProductoProveedor
    {
        [Key]
        [Column("id_producto_proveedor")]
        public int Id_Producto_Proveedor { get; set; }

        [Required]
        [Column("id_producto")]
        public int Id_Producto { get; set; }

        [Required]
        [Column("id_proveedor")]
        public int Id_Proveedor { get; set; }

        [ForeignKey("Id_Producto")]
        public required Producto Producto { get; set; }

        [ForeignKey("Id_Proveedor")]
        public required Proveedor Proveedor { get; set; }
    }
}