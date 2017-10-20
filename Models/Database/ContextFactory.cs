using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using Models.AppSettings;
using Newtonsoft.Json;

namespace Models {
    public class ContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext> {
        public static string ConnectionString { get; set; }

        public static ApplicationDbContext Create() {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
#if DEBUG
            GetConnectionStringFromSettings();
#endif
            builder.UseSqlServer(ConnectionString);
            return new ApplicationDbContext(builder.Options);
        }

        public ApplicationDbContext CreateDbContext(string[] args) {
            return Create();
        }

        private static void GetConnectionStringFromSettings() {
            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = JsonConvert.DeserializeObject<AppSetting>(File.ReadAllText("../Afisha/appsettings.Development.json"))
                                              .ConnectionStrings.DefaultConnection;
        }
    }
}
