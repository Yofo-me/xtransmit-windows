﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Utility;
using XTransmit.ViewModel.Control;
using XTransmit.ViewModel.Model;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-08-08
     */
    public class ServerConfigVModel : BaseViewModel
    {
        public ServerProfileView ServerInfoData { get; private set; }
        public ItemInfo[] ServerIPData { get; private set; }

        private bool vIsFetching = false;
        public bool IsFetching
        {
            get => vIsFetching;
            private set
            {
                vIsFetching = value;
                OnPropertyChanged("IsFetching");
            }
        }

        // language
        private static readonly string sr_title = (string)Application.Current.FindResource("dialog_server_title");
        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(ServerProfileView serverInfo)
        {
            ServerInfoData = serverInfo;

            ServerIPData = UpdateInfo();
        }

        private ItemInfo[] UpdateInfo()
        {
            return new ItemInfo[]
            {
                new ItemInfo{Label = "Created", Text = ServerInfoData.TimeCreated ?? sr_not_availabe},
                new ItemInfo{Label = "Last Ping (ms)", Text = ServerInfoData.Ping.ToString()},

                new ItemInfo{Label = "Country", Text = ServerInfoData.vServerProfile.vIPData?.country ?? sr_not_availabe},
                new ItemInfo{Label = "Region", Text = ServerInfoData.vServerProfile.vIPData?.region ?? sr_not_availabe},
                new ItemInfo{Label = "City", Text = ServerInfoData.vServerProfile.vIPData?.city ?? sr_not_availabe},
                new ItemInfo{Label = "Location", Text = ServerInfoData.vServerProfile.vIPData?.loc ?? sr_not_availabe},
                new ItemInfo{Label = "Org", Text = ServerInfoData.vServerProfile.vIPData?.org ?? sr_not_availabe},
                new ItemInfo{Label = "Host Name", Text = ServerInfoData.vServerProfile.vIPData?.hostname ?? sr_not_availabe},
            };
        }


        /** Commands ======================================================================================================
         */
        private bool IsNotFetching(object parameter) => !IsFetching;

        public RelayCommand CommandFetchData => new RelayCommand(FetchDataAsync, IsNotFetching);
        private async void FetchDataAsync(object parameter)
        {
            IsFetching = true;

            await Task.Run(() =>
            {
                ServerInfoData.UpdateIPInfo(true);
            });

            ServerIPData = UpdateInfo();
            OnPropertyChanged("ServerIPData");

            IsFetching = false;
            CommandManager.InvalidateRequerySuggested();
        }

        public RelayCommand CommandCloseOK => new RelayCommand(closeOK, IsNotFetching);
        private void closeOK(object parameter)
        {
            Window window = (Window)parameter;

            Match matchIP = Regex.Match(ServerInfoData.HostIP, RegexHelper.IPv4AddressRegex);
            if (!matchIP.Success)
            {
                new View.DialogPrompt(sr_title, sr_invalid_ip).ShowDialog();
                return;
            }
            if (ServerInfoData.Port < 100 || ServerInfoData.Port > 65535)
            {
                new View.DialogPrompt(sr_title, sr_invalid_port).ShowDialog();
                return;
            }

            window.DialogResult = true;
            window.Close();
        }

        public RelayCommand CommandCloseCancel => new RelayCommand(closeCancel);
        private void closeCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
