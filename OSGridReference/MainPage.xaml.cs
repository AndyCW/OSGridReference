using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DotNetCoords;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OSGridReference
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly uint? _desireAccuracyInMetersValue = 50;
        private Geolocator _geolocator;

        enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter?.ToString() == "protocol")
            {
                if (DoItButton.IsEnabled)
                {
                    DoItButton_Click(this, null);
                }
            }
            else
            {
                DoItButton.IsEnabled = true;
            }
        }


        private async void DoItButton_Click(object sender, RoutedEventArgs e)
        {
            DoItButton.IsEnabled = false;
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    NotifyUser("Waiting for update...", NotifyType.StatusMessage);

                    myProgressRing.IsActive = true;

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    _geolocator = new Geolocator { DesiredAccuracyInMeters = _desireAccuracyInMetersValue };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    _geolocator.StatusChanged += OnStatusChanged;

                    // Carry out the operation.
                    Geoposition pos = await _geolocator.GetGeopositionAsync();

                    // Convert to OS grid reference
                    var osRef = new OSRef(new LatLng(pos.Coordinate.Point.Position.Latitude, pos.Coordinate.Point.Position.Longitude));

                    //await new MessageDialog("OS grid reference = " +
                    //          osRef.ToSixFigureString()).ShowAsync();

                    var dlg = new GridRefDialog(osRef.ToSixFigureString());
                    dlg.Closed += (d, a) => DoItButton.IsEnabled = true;

                    myProgressRing.IsActive = false;

                    dlg.ShowAsync();
                    break;

                case GeolocationAccessStatus.Denied:
                    NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                    LocationDisabledMessage.Visibility = Visibility.Visible;
                    break;

                case GeolocationAccessStatus.Unspecified:
                    NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                    break;
            }
        }


        private void NotifyUser(string message, NotifyType type)
        {
            StatusMessage.Text = message;
        }

        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.
                LocationDisabledMessage.Visibility = Visibility.Collapsed;

                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        // Location platform is providing valid data.
                        ScenarioOutput_Status.Text = "Ready";
                        NotifyUser("Location platform is ready.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix. 
                        ScenarioOutput_Status.Text = "Initializing";
                        NotifyUser("Location platform is attempting to obtain a position.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        ScenarioOutput_Status.Text = "No data";
                        NotifyUser("Not able to determine the location.", NotifyType.ErrorMessage);
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        ScenarioOutput_Status.Text = "Disabled";
                        NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);

                        // Show message to the user to go to location settings.
                        LocationDisabledMessage.Visibility = Visibility.Visible;
                        break;

                    case PositionStatus.NotInitialized:
                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        ScenarioOutput_Status.Text = "Not initialized";
                        NotifyUser("No request for location is made yet.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NotAvailable:
                        // The location platform is not available on this version of the OS.
                        ScenarioOutput_Status.Text = "Not available";
                        NotifyUser("Location is not available on this version of the OS.", NotifyType.ErrorMessage);
                        break;

                    default:
                        ScenarioOutput_Status.Text = "Unknown";
                        NotifyUser(string.Empty, NotifyType.StatusMessage);
                        break;
                }
            });
        }

    }
}
