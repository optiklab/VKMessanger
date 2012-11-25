using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlXnaApp1.Infrastructure
{
    public static class CommonHelper
    {
        /// <summary>
        /// Generates md5 digest.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string DoDigest(string parameters)
        {
            MD5Managed md5 = new MD5Managed();
            md5.Initialize();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(parameters));

            int len = hash.Length;
            StringBuilder sb = new StringBuilder(len << 1);
            for (int i = 0; i < len; i++)
            {
                sb.Append(((hash[i] & 0xf0) >> 4).ToString("X"));
                sb.Append((hash[i] & 0x0f).ToString("X"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Chat Date formatter.
        /// </summary>
        public static string GetFormattedDate(long milliSeconds)
        {
            string formattedDate = string.Empty;

            try
            {
                DateTime time = START_DATE.AddMilliseconds(milliSeconds * 1000L);

                TimeZoneInfo localZone = TimeZoneInfo.Local;
                time = time.AddHours(localZone.BaseUtcOffset.TotalHours);

                if (time.Date == DateTime.Now.Date)
                    formattedDate = String.Format("{0:HH:mm}", time);
                else if (time.Year == DateTime.Now.Year)
                    formattedDate = String.Format("{0:dd/MMM} {0:HH:mm}", time, time);
                else
                    formattedDate = String.Format("{0:dd/MM/yy} {0:HH:mm}", time, time);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetFormattedDate failed:" + ex.Message);
            }

            return formattedDate;
        }

        /// <summary>
        /// Dialogs Date formatter.
        /// </summary>
        public static string GetFormattedDialogTime(long milliSeconds)
        {
            DateTime time = START_DATE.AddMilliseconds(milliSeconds * 1000L);
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            time = time.AddHours(localZone.BaseUtcOffset.TotalHours);

            string formattedDate;

            TimeSpan res = DateTime.Now - time;

            if (DateTime.Now.Date == time.Date)
                formattedDate = String.Format("{0:HH:mm}", time);
            else if (DateTime.Now.Day - 1 == time.Day)
                formattedDate = AppResources.Yesterday;
            else if (time.Year == DateTime.Now.Year)
                formattedDate = String.Format("{0:dd/MM}", time, time);
            else
                formattedDate = String.Format("{0:dd/MM/yy}", time, time);

            return formattedDate;
        }

        /// <summary>
        /// Chat status Date formatter.
        /// </summary>
        public static string GetFormattedStatusTime(long milliSeconds)
        {
            string result;

            DateTime time = START_DATE.AddMilliseconds(milliSeconds * 1000L);
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            time = time.AddHours(localZone.BaseUtcOffset.TotalHours);

            TimeSpan res = DateTime.Now - time;

            if (res.TotalMinutes < 60)
            {
                result = res.TotalMinutes.ToString("0") + AppResources.TotalMinutes;
            }
            else if (res.TotalDays < 1)
            {
                result = res.TotalHours.ToString("0") + AppResources.TotalHours;
            }
            else
            {
                result = res.TotalDays.ToString("0") + AppResources.TotalDays;
            }

            return result;
        }

        /// <summary>
        /// Message formatter.
        /// </summary>
        public static string GetFormattedMessage(string body)
        {
            string result = string.Empty;

            try
            {
                result = body.Replace("<br>", Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetFormattedMessage failed:" + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// String splitter.
        /// </summary>
        public static IList<int> SplitIntegersByComma(string toParse)
        {
            IList<int> ids = new List<int>();

            if (!string.IsNullOrEmpty(toParse))
            {
                string[] splitIds = toParse.Split(',');
                foreach (var stringId in splitIds)
                {
                    if (!string.IsNullOrEmpty(stringId))
                    {
                        ids.Add(Convert.ToInt32(stringId));
                    }
                }
            }

            return ids;
        }

        /// <summary>
        /// String creater.
        /// </summary>
        public static string GetCommaDelimetedIntegersString(IList<int> integers)
        {
            string result = string.Empty;

            foreach (var integer in integers)
            {
                result += integer.ToString() + ",";
            }

            result = result.TrimEnd(',');

            return result;
        }

        /// <summary>
        /// Attachment button icon selector.
        /// </summary>
        public static string GetUriOfAttachmentIcon()
        {
            string uri = string.Empty;
            int count = App.Current.EntityService.AttachmentPhotos.Count;

            if (!string.IsNullOrEmpty(App.Current.EntityService.AttachedLatitude) &&
                !string.IsNullOrEmpty(App.Current.EntityService.AttachedLongitude))
            {
                count += 1;
            }

            if (count == 1)
                return "/Images/Appbar_Icons/appbar.attachments-1.rest.png";
            else if (count == 2)
                return "/Images/Appbar_Icons/appbar.attachments-2.rest.png";
            else if (count == 3)
                return "/Images/Appbar_Icons/appbar.attachments-3.rest.png";
            else if (count == 4)
                return "/Images/Appbar_Icons/appbar.attachments-4.rest.png";
            else if (count == 5)
                return "/Images/Appbar_Icons/appbar.attachments-5.rest.png";
            else if (count == 6)
                return "/Images/Appbar_Icons/appbar.attachments-6.rest.png";
            else if (count == 7)
                return "/Images/Appbar_Icons/appbar.attachments-7.rest.png";
            else if (count == 8)
                return "/Images/Appbar_Icons/appbar.attachments-8.rest.png";
            else if (count == 9)
                return "/Images/Appbar_Icons/appbar.attachments-9.rest.png";
            else if (count >= 10)
                return "/Images/Appbar_Icons/appbar.attachments-10.rest.png";
            else
                return "/Images/Appbar_Icons/appbar.basecircle.rest.png";
        }

        /// <summary>
        /// DateTime values returned by the VRP service are encoded as a number
        /// of milliseconds since this date.
        /// </summary>
        private static readonly DateTime START_DATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
    }
}
