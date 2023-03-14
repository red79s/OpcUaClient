using System.ComponentModel.DataAnnotations;

namespace OpcUaClient.Entity
{
    public class OpcUrl
    {
        [Key]
        public long OpcUrlId { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
