using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Productos
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int Id_Producto { get; set; }

        [Required(ErrorMessage = "El código del producto es obligatorio")]
        [Column("codigo_producto")]
        public int Codigo_Producto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100)]
        [Column("nombre_producto")]
        public required string Nombre_Producto { get; set; }

        [StringLength(50)]
        [Column("marca")]
        public string? Marca { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Column("id_categoria")]
        public int Id_Categoria { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [StringLength(50)]
        [Column("unidad_medida")]
        public required string Unidad_Medida { get; set; }

        [ForeignKey("Id_Categoria")]
        public required Categorias.Categoria Categoria { get; set; }

        public ICollection<ProductoProveedor> ProductoProveedores { get; set; } = [];
        public ICollection<Actas.DetalleActa> DetallesActa { get; set; } = [];
        public ICollection<Movimientos.Movimiento> Movimientos { get; set; } = [];
    }
}