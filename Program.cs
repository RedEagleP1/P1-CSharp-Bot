using DiscordBot.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscordBot.Bot;
using DiscordBot.Pages.Home;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
builder.Services.AddSingleton(sp =>
{
    return new DiscordBotService(builder.Configuration.GetValue<string>("Discord:botToken"), sp);
});
builder.Services.AddHostedService(sp => sp.GetRequiredService<DiscordBotService>());
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UsePathBase("/bot");
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();