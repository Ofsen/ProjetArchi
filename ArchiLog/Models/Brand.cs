using ArchiLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchiLog.Models
{
    //[Table("nomDeTable")]
    public class Brand : BaseModel
    {
        [StringLength(50)]
        [Required()]
        public string? Name { get; set; }

        //[Column(Name="nomdeColonne")]
        public string? Slogan { get; set; }

        //public IEnumerable<Car> Cars { get; set; } erreur sérialisation
    }
}
