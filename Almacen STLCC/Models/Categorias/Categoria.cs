using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almacen_STLCC.Models.Categorias
{
    [Table("categorias")]
    public class Categoria
    {
        [Key]
        [Column("id_categoria")]
        public int Id_Categoria { get; set; }

        [StringLength(50)]
        [Column("nombre_categoria")]
        public string? Nombre_Categoria { get; set; }

        // Relación: Una categoría tiene muchos productos
        public ICollection<Productos.Producto> Productos { get; set; } = [];
    }
}