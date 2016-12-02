namespace KellysHydroponicExoticPlantGrowSystem.Views
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Controllers;
    using Restup.Webserver.File;
    using Restup.Webserver.Http;
    using Restup.Webserver.Rest;
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<PlantSensorsController>();
            var configuration = new HttpServerConfiguration()
                .ListenOnPort(5000)
                .RegisterRoute("api", restRouteHandler)
                .RegisterRoute(new StaticFileRouteHandler(@"HTML"))
                .EnableCors();
            try
            {
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
