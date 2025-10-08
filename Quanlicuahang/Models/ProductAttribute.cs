using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("ProductAttribute")]
    public class ProductAttribute : BasePrimary
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; 

        [MaxLength(255)]
        public string? Description { get; set; }

        public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    }
}
