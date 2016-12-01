using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using KellysHydroponicExoticPlantGrowSystem.Interfaces;
using KellysHydroponicExoticPlantGrowSystem.Services;

namespace KellysHydroponicExoticPlantGrowSystem
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Main", null);
            return Task.CompletedTask;
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)

        {
            // EventAggregator = new EventAggregator();

            Container.RegisterInstance(NavigationService);
            Container.RegisterInstance<IPlantMonitoringService>(new PlantMonitoringService(), new ContainerControlledLifetimeManager());
            return base.OnInitializeAsync(args);
        }
    }
}