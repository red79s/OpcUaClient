using System.Collections.Generic;
using System.Linq;
using OpcUaClient.Entity;

namespace OpcUaClient
{
    public class UrlConfigManager
    {
        public static List<OpcUrl> GetUrls()
        {
            using (var db = new MyDbContext())
            {
                var list = db.Urls.OrderBy(x => x.OpcUrlId).ToList();
                list.Reverse();
                return list;
            }
        }

        public static void AddUrl(string url)
        {
            using (var db = new MyDbContext())
            {
                foreach (var opcUrl in db.Urls)
                {
                    if (opcUrl.Url == url)
                        db.Urls.Remove(opcUrl);
                }
                db.Urls.Add(new OpcUrl() {Description = "url", Url = url});
                db.SaveChanges();
            }
        }

        public static void DeleteUrl(string url)
        {
            using (var db = new MyDbContext())
            {
                var row = db.Urls.FirstOrDefault(x => x.Url == url);
                if (row != null)
                {
                    db.Urls.Remove(row);
                    db.SaveChanges();
                }
            }
        }
    }
}
