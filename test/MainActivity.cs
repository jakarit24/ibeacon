using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using RadiusNetworks.IBeaconAndroid;

using Color = Android.Graphics.Color;
using Android.Bluetooth;
using Android.Runtime;
using System;
using Java.Lang;

namespace App1
{
    [Activity(Label = "iBeacon", MainLauncher = true, LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : Activity, IBeaconConsumer
    {
        private const string UUID = "e2c56db5-dffb-48d2-b060-d0f5a71096e0";
        //private const string UUID = "1c7b5c5d-dfab-1f62-841b-4776b8c78b67";
        private const string monkeyId = "blueberry";

        bool _paused;
        private BluetoothManager _manager;
        View _view;
        IBeaconManager _iBeaconManager;       
        RangeNotifier _rangeNotifier;    
        Region _rangingRegion;
        TextView _text;

        int _previousProximity;

        public MainActivity()
        {
            
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            _view = FindViewById<RelativeLayout>(Resource.Id.findTheMonkeyView);
            _text = FindViewById<TextView>(Resource.Id.monkeyStatusLabel);

            //////
            _iBeaconManager = IBeaconManager.GetInstanceForApplication(this);

            _iBeaconManager.SetForegroundScanPeriod(2000);
            _iBeaconManager.SetForegroundBetweenScanPeriod(2500);

            _rangeNotifier = new RangeNotifier();

            _rangingRegion = new Region(monkeyId, UUID, null, null);
            _iBeaconManager.Bind(this);

            _rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;

        }

        void RangingBeaconsInRegion(object sender, RangeEventArgs e)
        {
            if (e.Beacons.Count > 0)
            {
                var beacon = e.Beacons.FirstOrDefault();
                var message = string.Empty;

                switch ((ProximityType)beacon.Proximity)
                {
                    case ProximityType.Immediate:
                        UpdateDisplay("You found the Estimote!", Color.Green);
                        break;
                    case ProximityType.Near:
                        UpdateDisplay("You're getting warmer", Color.Yellow);
                        break;
                    case ProximityType.Far:
                        UpdateDisplay("You're freezing cold", Color.Blue);
                        break;
                    case ProximityType.Unknown:
                        UpdateDisplay("I'm not sure how close you are to the Estimote", Color.Red);
                        break;
                }

                _previousProximity = beacon.Proximity;
            }
        }

        #region IBeaconConsumer impl
        public void OnIBeaconServiceConnect()
        {         
            _iBeaconManager.SetRangeNotifier(_rangeNotifier);       
            _iBeaconManager.StartRangingBeaconsInRegion(_rangingRegion);
        }
        #endregion

        private void UpdateDisplay(string message, Color color)
        {
            RunOnUiThread(() =>
            {
                _text.Text = message;
                _view.SetBackgroundColor(color);
            });
        }

        private void ShowNotification()
        {
            var resultIntent = new Intent(this, typeof(MainActivity));
            resultIntent.AddFlags(ActivityFlags.ReorderToFront);
            var pendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
            var notificationId = Resource.String.monkey_notification;

            var builder = new Notification.Builder(this)
                //.SetSmallIcon(Resource.Drawable.Xamarin_Icon)
                .SetContentTitle(this.GetText(Resource.String.app_label))
                .SetContentText(this.GetText(Resource.String.monkey_notification))
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            var notification = builder.Build();

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(notificationId, notification);
        }

        private void ShowNotification1()
        {
            var resultIntent = new Intent(this, typeof(MainActivity));
            resultIntent.AddFlags(ActivityFlags.ReorderToFront);
            var pendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
            var notificationId = Resource.String.monkey_notification;

            var builder = new Notification.Builder(this)
                //.SetSmallIcon(Resource.Drawable.Xamarin_Icon)
                .SetContentTitle(this.GetText(Resource.String.app_label))
                .SetContentText(this.GetText(Resource.String.monkey_notification))
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            var notification = builder.Build();

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(notificationId, notification);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _rangeNotifier.DidRangeBeaconsInRegionComplete -= RangingBeaconsInRegion;   
            _iBeaconManager.StopRangingBeaconsInRegion(_rangingRegion);
            _iBeaconManager.UnBind(this);
        }
    }
}