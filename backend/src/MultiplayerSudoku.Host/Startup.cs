using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiplayerSudoku.Application;
using MultiplayerSudoku.Application.Contract;
using MultiplayerSudoku.Host.Middlewares;

namespace MultiplayerSudoku.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IGameAgent, GameAgent>();
            services.AddSingleton<ILeaderboardService, Leaderboard>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseMvc();
            app.UseWebSockets();
            app.UseMiddleware<SudokuSocketMiddleware>();
        }
    }
}
