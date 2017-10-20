using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Models.Extensions {
    public static class AppBuilderExtensions {
        public static void UseMigrations(this IApplicationBuilder _app) {
            using (var ctx = ContextFactory.Create()) {
                ctx.Database.Migrate();
            }
        }
    }
}
