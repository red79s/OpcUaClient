using System.Linq;
using OpcUaClient.Entity;

namespace OpcUaClient
{
    public class AppSettingsManager
    {
        public string Get(string key)
        {
            using (var context = new MyDbContext())
            {
                var setting = context.AppSettings.FirstOrDefault(t => t.AppSettingId == key);
                return setting == null ? "" : setting.Value;
            }
        }

        public void Set(string key, string value)
        {
            using (var context = new MyDbContext())
            {
                var setting = context.AppSettings.FirstOrDefault(t => t.AppSettingId == key);
                if (setting == null)
                {
                    setting = new AppSetting() { AppSettingId = key};
                    context.AppSettings.Add(setting);
                }
                setting.Value = value;

                context.SaveChanges();
            }
        }
    }
}
