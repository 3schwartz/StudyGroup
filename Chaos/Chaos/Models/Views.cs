namespace Chaos.Models
{
    public class ChaosView
    {
        public int ChaosItemId { get; set; }
        public string Name { get; set; }
        public IList<ChaosLink> Links { get; set; }
    }
}
