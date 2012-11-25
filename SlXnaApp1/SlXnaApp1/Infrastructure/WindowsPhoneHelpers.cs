using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Net.NetworkInformation;

namespace SlXnaApp1.Infrastructure
{
    public static class WindowsPhoneHelpers
    {
        public static bool IsAnyNetworkAvailable()
        {
            ////Check if network is available
            //bool isNetworkAvailable = DeviceNetworkInformation.IsNetworkAvailable;

            ////Check if Cell Data connection is available
            //bool isCellDataAvailable = DeviceNetworkInformation.IsCellularDataEnabled;

            ////Check if the cell data connection is in ROaming
            //bool isCellDataRoaming = DeviceNetworkInformation.IsCellularDataRoamingEnabled;

            ////Check if WiFi Network is available
            //bool isWiFiEnabled = DeviceNetworkInformation.IsWiFiEnabled;

            bool isConnected = NetworkInterface.GetIsNetworkAvailable();

            //#if DEBUG
            //            isConnected = false;
            //#endif

            return isConnected;
        }

        public static bool IsItemVisible(ListBox listbox, int index)
        {
            var listboxRectangle = new Rect(new Point(0, 0), listbox.RenderSize);

            double visiblePercent = 0;

            ListBoxItem item = listbox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            if (item != null)
            {
                var itemTransform = item.TransformToVisual(listbox);
                var itemRectangle = itemTransform.TransformBounds(new Rect(new Point(0, 0), item.RenderSize));
                itemRectangle.Intersect(listboxRectangle);

                if (!itemRectangle.IsEmpty)
                {
                    visiblePercent = itemRectangle.Height / item.RenderSize.Height * 100;
                }
            }

            if (visiblePercent > 0)
                return true;
            else
                return false;
        }

        public static bool IsFirstItemVisible(ListBox listbox)
        {
            if (listbox.Items.Count == 0)
                return false;

            var listboxRectangle = new Rect(new Point(0, 0), listbox.RenderSize);

            double visiblePercent = 0;

            ListBoxItem item = listbox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
            if (item != null)
            {
                var itemTransform = item.TransformToVisual(listbox);
                var itemRectangle = itemTransform.TransformBounds(new Rect(new Point(0, 0), item.RenderSize));
                itemRectangle.Intersect(listboxRectangle);

                if (!itemRectangle.IsEmpty)
                {
                    visiblePercent = itemRectangle.Height / item.RenderSize.Height * 100;
                }
            }

            if (visiblePercent > 0)
                return true;
            else
                return false;
        }

        public static string GetDeviceModel()
        {
           string model = null;
           object theModel = null;
  
           if (Microsoft.Phone.Info.DeviceExtendedProperties.TryGetValue("DeviceName", out theModel))
              model = theModel as string;

           return model;
        }

        public static string GetOSVersion()
        {
            return System.Environment.OSVersion.Version.Major.ToString() +
                "." + System.Environment.OSVersion.Version.Minor.ToString();
        }
    }
}
