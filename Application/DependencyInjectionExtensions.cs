using Application.Services;
using Application.Services.MovementSimulator;
using Application.Services.Products;
using Application.Services.Supplier;
using Domain.Interfaces;
using Domain.Interfaces.MovementSimulator;
using Domain.Interfaces.Products;
using Domain.Interfaces.Suppliers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using WebUI.Shared.UIControls;

namespace Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSupplierServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISupplierProductService, SupplierProductService>();
        serviceCollection.AddScoped<ISupplierService, SupplierService>();
        serviceCollection.AddScoped<ISupplierCategoryService, SupplierCategoryService>();
        serviceCollection.AddScoped<IProductCategoryService, ProductCategoryService>();

        return serviceCollection;
    }

    public static IServiceCollection AddProductServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IProductService, ProductService>();
        serviceCollection.AddScoped<ProductPriceHistoryService>();

        return serviceCollection;
    }

    public static IServiceCollection AddCountryService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICountryService, CountryService>();

        return serviceCollection;
    }

    public static IServiceCollection AddDepartmentLocationService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDepartmentLocationService, DepartmentLocationService>();

        return serviceCollection;
    }

    public static IServiceCollection AddToastNotifier(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<INotifier, Notifier>();

        return serviceCollection;
    }
    
    public static IServiceCollection AddMovementService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IMovementService, MovementService>();
        
        return serviceCollection;
    }
        
    public static IServiceCollection AddAiChatService(
        this IServiceCollection serviceCollection, 
        string? openApiKey,
        string? model)
    {
        serviceCollection.AddSingleton<IChatClient>(
            new OpenAI.Chat.ChatClient(apiKey: openApiKey, model: model).AsIChatClient());
        
        serviceCollection.AddScoped<IAiChatService, AiChatService>();
        
        return serviceCollection;       
    }
    
}