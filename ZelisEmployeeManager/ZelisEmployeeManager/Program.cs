using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ZelisEmployeeManager.Components;
using ZelisEmployeeManager.Data;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// HttpClient for Blazor components to call this app's API
// ReadME.MD contains details on this decision
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri) });

// controller services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ZelisDBConnString")));



var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // automatically apply "update-database" on app start
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();


app.MapOpenApi();   // serves /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));


app.MapStaticAssets();

app.MapControllers(); // maps route attributes on controllers

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
