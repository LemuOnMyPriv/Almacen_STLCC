using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Almacen_STLCC.Models.Productos;

namespace Almacen_STLCC.Models.Actas
{
    [Table("detalle_acta")]
    public class DetalleActa
    {
        [Key]
        [Column("id_detalle")]
        public int Id_Detalle { get; set; }

        [Required]
        [Column("id_acta")]
        public int Id_Acta { get; set; }

        [Required]
        [Column("id_producto")]
        public int Id_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(20,2)")]
        public decimal? Precio_Unitario { get; set; }

        [Column("precio_con_isv", TypeName = "decimal(20,2)")]
        public decimal? Precio_Con_Isv { get; set; }

        [StringLength(100)]
        [Column("requisicion")]
        public string? Requisicion { get; set; }

        // Navegación
        [ForeignKey("Id_Acta")]
        public required Acta Acta { get; set; }

        [ForeignKey("Id_Producto")]
        public required Producto Producto { get; set; }

    }
}