namespace ArchiLibrary.Extensions.Models
{
    public class ParamsModel
    {
        public string? Sort { get; set; }
        public string? Desc { get; set; }
        public string? Range { get; set; }
        public string? Fields { get; set; }
    }

    // api/v1/brand?asc=name&desc=id&range=0-10
}
