namespace ArchiLibrary.Extensions.Models
{
    public class ParamsModel
    {
        public string? Sort { get; set; }
        public string? Desc { get; set; }
        public string? Range { get; set; }
        public string? Fields { get; set; }
        public IDictionary<string, string>? Keys { get; set; } = new Dictionary<string, string>();
    }
}
