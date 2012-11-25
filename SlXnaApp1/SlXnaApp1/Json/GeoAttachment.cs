using System;
using System.Runtime.Serialization;
using SlXnaApp1.Entities;
using System.Diagnostics;
using System.Globalization;

namespace SlXnaApp1.Json
{
    [DataContract(Name="geo")]
    public class GeoAttachment
    {
        #region Public properties

        [DataMember(Name = "coordinates")]
        public string Coordinates { get; set; }

        public double Latitude
        {
            get
            {
                if (_latitude == 0)
                    _SplitCoordinates(Coordinates);

                return _latitude;
            }
        }

        public double Longitude
        {
            get
            {
                if (_longitude == 0)
                    _SplitCoordinates(Coordinates);

                return _longitude;
            }
        }

        #endregion

        #region Private method

        private void _SplitCoordinates(string coordinates)
        {
            if (!string.IsNullOrEmpty(Coordinates))
            {
                string[] values = Coordinates.Split(' ');
                if (values.Length == 2 &&
                    !string.IsNullOrEmpty(values[0]) &&
                    !string.IsNullOrEmpty(values[1]))
                {
                    try
                    {
                        NumberFormatInfo provider = new NumberFormatInfo();
                        _latitude = Convert.ToDouble(values[0], provider);
                        _longitude = Convert.ToDouble(values[1], provider);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Converting string to double for Longitude and Latitude failed.");
                    }
                }
            }
        }

        #endregion

        #region Private fields

        private double _latitude;
        private double _longitude;

        #endregion
    }
}
