using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Movimientos
{
    [Table("movimientos")]
    public class Movimiento
    {
        [Key]
        [Column("id_movimiento")]
        public int Id_Movimiento { get; set; }

        [Required]
        [Column("id_producto")]
        public int Id_Producto { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [StringLength(10)]
        [Column("tipo_movimiento")]
        public required string Tipo_Movimiento { get; set; } // "entrada", "salida", "ajuste"

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("id_acta")]
        public int? Id_Acta { get; set; }

        // Navegación
        [ForeignKey("Id_Producto")]
        public required Productos.Producto Producto { get; set; }

        [ForeignKey("Id_Acta")]
        public Actas.Acta? Acta { get; set; }
    }
}