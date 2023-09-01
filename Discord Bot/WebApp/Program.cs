using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using Models;
using Discord.Rest;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var pathBase = builder.Configuration.GetValue<string>("PathBase");
Console.WriteLine(pathBase);
await DiscordREST.Init(builder.Configuration.GetValue<string>("Discord:botToken"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = "Discord";
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddOAuth("Discord", options =>
    {
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        options.Scope.Add("guilds.members.read");

        options.CallbackPath = "/auth/authorizeDiscord";
        options.ClientId = builder.Configuration.GetValue<string>("Discord:clientID");
        options.ClientSecret = builder.Configuration.GetValue<string>("Discord:clientSecret");

        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        var guildID = builder.Configuration.GetSection("Discord:authentication:guildID").Value;
        options.UserInformationEndpoint = $"https://discord.com/api/users/@me/guilds/{guildID}/member";

        options.AccessDeniedPath = "/Account/AuthenticationFailed";

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                string name = "";
                string nameIdentifier = "";
                string role = "";

                if (user.GetProperty("user").TryGetProperty("id", out var value))
                {
                    name = user.GetProperty("user").GetProperty("id").ToString();
                    nameIdentifier = user.GetProperty("user").GetProperty("username").ToString();

                    var userRoles = JsonConvert.DeserializeObject<string[]>(user.GetProperty("roles").GetRawText());
                    foreach (string ur in userRoles)
                    {
                        if (builder.Configuration.GetSection("Discord:authentication:roles").Get<string[]>().Contains(ur))
                        {
                            role = ur;
                            break;
                        }
                    }
                }

                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, name));
                context.Identity.AddClaim(new Claim(ClaimTypes.Name, nameIdentifier));
                context.Identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        };

    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Allowed", policy =>
    {
        policy.RequireRole(builder.Configuration.GetSection("Discord:authentication:roles").Get<string[]>());
    });
});
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UsePathBase(pathBase);
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
