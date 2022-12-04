using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace G24_BWallet_Backend.Models
{
    [Table("ActivityIcon")]
    public class ActivityIcon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
    }
}
