using GeneratorWPF.Services;
using GeneratorWPF.Utils;
using GeneratorWPF.View._Entity;
using GeneratorWPF.ViewModel;
using GeneratorWPF.ViewModel._Dto;
using GeneratorWPF.ViewModel._Entity; 
using Microsoft.Extensions.DependencyInjection;  
using System.Windows; 

namespace GeneratorWPF;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;
    public App()
    {
        IServiceCollection services = new ServiceCollection();
        
        services.AddSingleton<INavigationService, NavigationService>();
 
        services.AddSingleton<MainWindow>(); 

        services.AddTransient<MainWindowVM>();
        services.AddTransient<HomeVM>();
        services.AddTransient<EntityHomeVM>();
        services.AddTransient<EntityDetailVM>();
        services.AddTransient<DtoHomeVM>();
        services.AddTransient<DtoDetailVM>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
         
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
