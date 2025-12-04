using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Actas
{
    [Table("anexos")]
    public class Anexo
    {
        [Key]
        [Column("id_anexo")]
        public int Id_Anexo { get; set; }

        [Required]
        [Column("id_acta")]
        public int Id_Acta { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
        [StringLength(255)]
        [Column("nombre_archivo")]
        public required string Nombre_Archivo { get; set; }

        [Required(ErrorMessage = "El tipo de archivo es obligatorio")]
        [StringLength(50)]
        [Column("tipo_archivo")]
        public required string Tipo_Archivo { get; set; }

        [Required]
        [StringLength(500)]
        [Column("ruta_minio")]
        public required string Ruta_Minio { get; set; }

        [Required]
        [StringLength(100)]
        [Column("bucket_minio")]
        public string Bucket_Minio { get; set; } = "anexos-inventario";

        [Required]
        [Column("tamaño_kb")]
        public int Tamaño_Kb { get; set; }

        [Required]
        [Column("fecha_subida")]
        public DateTime Fecha_Subida { get; set; }

        [ForeignKey("Id_Acta")]
        public required Acta Acta { get; set; }
    }
}