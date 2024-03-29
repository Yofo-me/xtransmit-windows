﻿using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using XTransmit.Model.Network;
using XTransmit.Utility;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-08-02
     */
    class ContentNetworkVModel : BaseViewModel
    {
        private bool vIsActivated = false;
        public bool IsActivated
        {
            get { return vIsActivated; }
            set
            {
                vIsActivated = value;
                if (vIsActivated) adapterSpeedMeter.Start();
                else adapterSpeedMeter.Stop();
            }
        }

        public List<string> NetworkInterfaceAll { get; private set; } // interface descriptions
        public string NetworkInterfaceSelected
        {
            get { return App.GlobalConfig.NetworkAdapter; }
            set { App.GlobalConfig.NetworkAdapter = value; }
        }

        public SeriesCollection ChartSeries { get; set; }
        public Func<double, string> ChartXFormatter { get; set; }
        public Func<double, string> ChartYFormatter { get; set; }

        private readonly List<NetworkInterface> adapterList; // network interfaces
        private readonly BandwidthMeter adapterSpeedMeter;

        private static readonly string sr_download = (string)Application.Current.FindResource("_download");
        private static readonly string sr_upload = (string)Application.Current.FindResource("_upload");

        public ContentNetworkVModel()
        {
            // init live chart
            var dayConfig = LiveCharts.Configurations.Mappers.Xy<BandwidthInfo>()
                .X(dayModel => (double)dayModel.Time.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dayModel => dayModel.Value);

            ChartSeries = new SeriesCollection(dayConfig)
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = sr_download,
                    LineSmoothness = 1,
                    PointGeometry = null,
                    Stroke = Application.Current.Resources[key:"PrimaryHueMidBrush"] as Brush,
                    Values = new ChartValues<BandwidthInfo>(),
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Title = sr_upload,
                    LineSmoothness = 1,
                    PointGeometry = null,
                    Stroke = Application.Current.Resources[key:"SecondaryAccentBrush"] as Brush,
                    Values = new ChartValues<BandwidthInfo>(),
                }
            };

            ChartXFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("HH:mm:ss");
            ChartYFormatter = value => $"{TextUtil.GetBytesReadable((long)value)}/s";

            // get network interfaces
            adapterList = NetworkUtil.GetValidNetworkInterface();
            adapterList.Sort(comparison: NetworkUtil.CompareNetworkInterfaceBySpeed);
            adapterList.Reverse();

            NetworkInterfaceAll = new List<string>();
            foreach (NetworkInterface adapter in adapterList)
                NetworkInterfaceAll.Add(adapter.Description);

            // init speed meter
            adapterSpeedMeter = new BandwidthMeter(AdapterSpeedMeter_UpdateSpeed);

            if (NetworkInterfaceAll.Count > 0)
            {
                if (NetworkInterfaceAll.FirstOrDefault(item => item == NetworkInterfaceSelected) == null)
                    NetworkInterfaceSelected = adapterList[0].Description;

                IsActivated = true;
            }

            // stop SpeedMeter on closing
            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // case "hide" 
            if (Application.Current.MainWindow.IsVisible)
                return;

            adapterSpeedMeter.Stop();
        }


        /** Network interfaces --------------------------------------------------------
         */
        public void UpdateNetworkInterface()
        {
            if (NetworkInterfaceSelected != null)
            {
                NetworkInterface adapterSelected = adapterList.FirstOrDefault(x => x.Description == NetworkInterfaceSelected);
                adapterSpeedMeter.SetAdapter(adapterSelected);
            }
        }


        /** Actions ===================================================================
         */
        private void AdapterSpeedMeter_UpdateSpeed(long[] values)
        {
            DateTime now = DateTime.Now;
            ChartSeries[0].Values.Add(new BandwidthInfo(now, values[0]));
            ChartSeries[1].Values.Add(new BandwidthInfo(now, values[1]));

            // Remove data older than 30 seconds
            if (ChartSeries[0].Values.Count > 30) ChartSeries[0].Values.RemoveAt(0);
            if (ChartSeries[1].Values.Count > 30) ChartSeries[1].Values.RemoveAt(0);

            OnPropertyChanged("ValueSent");
            OnPropertyChanged("ValueReceived");
        }

        /** Commands ==================================================================
         */
        public RelayCommand CommandToggleActivate => new RelayCommand(toggleActivate);
        private void toggleActivate(object obj)
        {
            IsActivated = !IsActivated;
            OnPropertyChanged("IsActivated");
        }
    }
}
