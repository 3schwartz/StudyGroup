namespace RestServer.Models
{
    public class Droid
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Episode> Episodes { get; set; }
        public string PrimaryFunction { get; set; }
    }
}
