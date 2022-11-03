namespace ArchiLibrary.Extensions.Models
{
    public class ParamsModel
    {
        public string? Asc { get; set; }
        public string? Desc { get; set; }
        public string? Range { get; set; }
    }

    // api/v1/brand?asc=name&desc=id&range=0-10
}
