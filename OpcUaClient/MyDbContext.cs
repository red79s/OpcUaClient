using OpcUaClient.Entity;
using System.Data.Entity;

namespace OpcUaClient
{
    public class MyDbContext : DbContext
    {
        public DbSet<OpcUrl> Urls { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
    } 
}
