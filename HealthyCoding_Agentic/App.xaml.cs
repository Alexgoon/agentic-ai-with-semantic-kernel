using System;
using System.Windows;
using HealthyCoding_Agentic.Infrastructure;
using HealthyCoding_Agentic.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HealthyCoding_Agentic;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        var host = RegisterServices().Build();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    IHostBuilder RegisterServices() {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<IAgentService, AgentService>();
                services.AddSingleton<IDispatcherService>(new DispatcherService(Dispatcher));
            });
        return builder;
    }
}
