using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace G24_BWallet_Backend.Models
{

    [Table("ProofImage")]
    public class ProofImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ImageType { get; set; }
        public string ImageLink { get; set; }
        public int ModelId { get; set; }
    }
}
