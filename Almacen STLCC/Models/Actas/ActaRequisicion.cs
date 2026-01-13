using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Actas
{
    [Table("acta_requisicion")]
    public class ActaRequisicion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El acta es obligatoria")]
        [Column("id_acta")]
        public int Id_Acta { get; set; }

        [Required(ErrorMessage = "La requisición es obligatoria")]
        [StringLength(100, ErrorMessage = "La requisición no puede superar los 100 caracteres")]
        [Column("requisicion")]
        public required string Requisicion { get; set; }

        [ForeignKey("Id_Acta")]
        public required Acta Acta { get; set; }
    }
}