using System;
using Windows.UI.Xaml;
using KellysHydroponicExoticPlantGrowSystem.Controllers;
using Restup.Webserver.File;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;

namespace KellysHydroponicExoticPlantGrowSystem.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<PlantSensorsController>();

            try
            {
                var configuration = new HttpServerConfiguration()
                    .ListenOnPort(5000)
                    .RegisterRoute("api", restRouteHandler)
                    .RegisterRoute(new StaticFileRouteHandler(@"HTML"))
                    .EnableCors();

                var httpServer = new HttpServer(configuration);
                await httpServer.StartServerAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}