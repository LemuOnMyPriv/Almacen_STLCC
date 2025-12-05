using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Auditoria
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("id_auditoria")]
        public int Id_Auditoria { get; set; }

        [Required]
        [StringLength(100)]
        [Column("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("tabla")]
        public string Tabla { get; set; } = string.Empty;

        [Required]
        [Column("id_registro")]
        public int Id_Registro { get; set; }

        [Column("descripcion", TypeName = "TEXT")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime Fecha_Hora { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Column("ip_address")]
        public string? Ip_Address { get; set; }
    }
}