using ApexCharts;
using Application;
using Application.Services;
using Blazored.Toast;
using Domain.Interfaces;
using Infrastructure;
using Sysinfocus.AspNetCore.Components;
using WebUI.Components;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLucideIcons();
builder.Services.AddBlazoredToast();
builder.Services.AddSysinfocus();
builder.Services.AddApexCharts();

// Add database.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddSupplierServices()
    .AddProductServices()
    .AddCountryService()
    .AddDepartmentLocationService()
    .AddMovementService()
    .AddToastNotifier();

// builder.Services.AddAiChatService(openApiKey: config["OpenAIKey"], model: config["ModelName"]);

builder.Services.AddHttpClient<ICountryService, CountryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();