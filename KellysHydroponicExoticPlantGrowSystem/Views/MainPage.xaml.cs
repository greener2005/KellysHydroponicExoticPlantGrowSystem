using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using KellysHydroponicExoticPlantGrowSystem.Controllers;
using Restup.Webserver.File;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KellysHydroponicExoticPlantGrowSystem.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
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
                
                throw;
            }
        }
    }
}
