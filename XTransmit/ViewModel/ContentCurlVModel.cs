﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using XTransmit.Model.Curl;
using XTransmit.Model.IPAddress;
using XTransmit.Model.UserAgent;
using XTransmit.View;

namespace XTransmit.ViewModel
{
    /**
     * NOTE
     * Optimize the save action
     * 
     * Updated: 2019-09-28
     */
    public class ContentCurlVModel : BaseViewModel
    {
        public ObservableCollection<SiteProfile> SiteListOC { get; private set; }

        public ContentCurlVModel()
        {
            // load IPAddress and UserAgent data
            IPManager.Load(App.FileIPAddressXml);
            UAManager.Load(App.FileUserAgentXml);
            SiteManager.Load(App.FileCurlXml);

            // load curl data
            SiteListOC = new ObservableCollection<SiteProfile>(SiteManager.SiteList);

            // set list display grouping
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(SiteListOC);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Website");
            collectionView.GroupDescriptions.Add(groupDescription);
        }

        /** Actions ===============================================================================
         */
        private void OnSaveProfile(SiteProfile profile)
        {
            SiteProfile profileNew = profile.Copy();
            SiteProfile profileOld = SiteListOC.FirstOrDefault(item => item.Title == profileNew.Title && item.Website == profileNew.Website);

            if (profileOld != null)
            {
                int index = SiteListOC.IndexOf(profileOld);
                SiteListOC[index] = profileNew;
            }
            else
            {
                SiteListOC.Add(profileNew);
            }

            // convert to list and save
            List<SiteProfile> siteProfiles = new List<SiteProfile>(SiteListOC);
            SiteManager.SaveNew(siteProfiles);
        }

        /** Commands ==============================================================================
         */
        private bool IsSelectionProfile(object selected) => (selected is SiteProfile);

        // new profile
        public RelayCommand CommandNewProfile => new RelayCommand(NewProfile);
        private void NewProfile(object parameter)
        {
            new WindowCurlPlay(new SiteProfile(), OnSaveProfile).Show();
        }

        // delete profile
        public RelayCommand CommandDeleteProfile => new RelayCommand(DeleteProfile, IsSelectionProfile);
        private void DeleteProfile(object selected)
        {
            if (selected is SiteProfile profileDelete)
            {
                SiteProfile profile = SiteListOC.FirstOrDefault(item => item.Title == profileDelete.Title && item.Website == profileDelete.Website);
                if (profile != null)
                {
                    SiteListOC.Remove(profile);

                    // convert to list and save
                    List<SiteProfile> siteProfiles = new List<SiteProfile>(SiteListOC);
                    SiteManager.SaveNew(siteProfiles);
                }
            }
        }

        // launch
        public RelayCommand CommandLaunchProfile => new RelayCommand(LaunchProfile, IsSelectionProfile);
        private void LaunchProfile(object selected)
        {
            if (selected is SiteProfile profile)
            {
                new WindowCurlPlay(profile.Copy(), OnSaveProfile).Show();
            }
        }

        public RelayCommand CommandViewIPAddress => new RelayCommand(ViewIPAddress);
        private void ViewIPAddress(object obj)
        {
            new WindowIPAddress().Show();
        }

        public RelayCommand CommandViewUserAgent => new RelayCommand(ViewUserAgent);
        private void ViewUserAgent(object obj)
        {
            new WindowUserAgent().Show();
        }
    }
}
