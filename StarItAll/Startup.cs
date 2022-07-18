namespace StarItAll;

using System.Security;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        if (Environment.GetEnvironmentVariable("ClientId") == null)
        {
            throw new SecurityException("No ClientId set");
        }

        if (Environment.GetEnvironmentVariable("ClientSecret") == null)
        {
            throw new SecurityException("No ClientSecret set");
        }
        services.AddAuthentication(options => {})
            .AddGitHub(options =>
            {
                options.ClientId = Environment.GetEnvironmentVariable("ClientId");
                options.ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}