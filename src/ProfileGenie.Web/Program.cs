using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ProfileGenie.Data;
using ProfileGenie.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (telemetry, health checks, etc.)
builder.AddServiceDefaults();

// Add database context
// TODO: Configure PostgreSQL connection via Aspire

// Add Razor components
builder.Services.AddRazorComponents();

// Add application services
// TODO: Register JobService, ScoringEngine, etc.

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map health check endpoints
app.MapDefaultEndpoints();

// Map Razor components
app.MapRazorComponents<ProfileGenie.Web.Components.App>();

app.Run();
