using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Almacen_STLCC.Models.Movimientos;

namespace Almacen_STLCC.Models.Actas
{
    [Table("actas")]
    public class Acta
    {
        [Key]
        [Column("id_acta")]
        public int Id_Acta { get; set; }

        [Required(ErrorMessage = "El Numero de Acta es obligatorio")]
        [StringLength(100)]
        [Column("numero_acta")]
        public required string Numero_Acta { get; set; }

        [StringLength(100)]
        [Column("orden_compra")]
        public string? Orden_Compra { get; set; }

        [Required(ErrorMessage = "El código F01 es obligatorio")]
        [StringLength(100)]
        [Column("f01")]
        public required string F01 { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Column("id_proveedor")]
        public int Id_Proveedor { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        // Navegación
        [ForeignKey("Id_Proveedor")]
        public required Proveedores.Proveedor Proveedor { get; set; }

        // Relaciones
        public ICollection<DetalleActa> DetallesActa { get; set; } = [];
        public ICollection<Movimiento> Movimientos { get; set; } = [];
        public ICollection<Anexo> Anexos { get; set; } = [];
        public ICollection<ActaRequisicion> Requisiciones { get; set; } = [];
    }
}