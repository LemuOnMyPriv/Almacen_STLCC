using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200)]
        [Column("nombre")]
        public string NombreProd { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [StringLength(100)]
        [Column("categoria")]
        public string CategoriaProd { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Column("cantidad")]
        public int CantidadProd { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [StringLength(200)]
        [Column("proveedor")]
        public string Proveedor { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}