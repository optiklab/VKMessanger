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

namespace SlXnaApp1.Api
{
    public class GetUserSettings: APIRequest
    {
        public GetUserSettings()
            : base("getUserSettings")
        {
            base.AddParameter("uid", "2955448");

            base.SetSuccessHandler(_ParseResponse);
        }

        private void _ParseResponse(string response)
        {
            string r = response;
        }
    }
}
