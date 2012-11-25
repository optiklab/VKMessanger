using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class GetStatInfo : APIRequest
    {
        public GetStatInfo(Action<IList<int>, IList<FriendRequestEntity>> callback)
            : base("execute")
        {
            base.AddParameter("code", "var a=API.messages.get({count:100,filters:1,preview_length:1});" +
                                      "var p=API.friends.getRequests({need_messages:1,need_mutual:1});" +
                                      "return{a:a,p:p};");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(new List<int>(), new List<FriendRequestEntity>());
                else
                {
                    response = _FixResponseArrayString(response);
                    response = _FixResponseProfilesString(response);

                    var result = SerializeHelper.Deserialise<StatResponse>(response);

                    List<int> mids = new List<int>();
                    if (result.StatRResponse.Messages != null)
                        mids = result.StatRResponse.Messages.Select(x => x.Mid).ToList();

                    _callback(mids, result.StatRResponse.FriendRequests != null ? result.StatRResponse.FriendRequests : new List<FriendRequestEntity>());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response: " + response + " from GetStatInfo failed:" + ex.Message);

                _callback(new List<int>(), new List<FriendRequestEntity>());
            }
        }

        private string _FixResponseArrayString(string response)
        {
            int emptyArray = response.IndexOf("[0]");

            if (emptyArray > -1)
            {
                response = response.Remove(18, 1);
            }
            else
            {
                int firstComma = response.IndexOf(",");
                if (firstComma < 18 || firstComma == -1) //18 - is length of //{"response":{"a":[
                    return response; // Don't need to fix.

                response = response.Remove(18, firstComma - 17);
            }

            return response;
        }

        private string _FixResponseProfilesString(string response)
        {
            int start = response.IndexOf(",\"p\":[");
            int emptyArray = response.IndexOf(",\"p\":[0]");

            if (emptyArray > -1)
            {
                response = response.Remove(emptyArray + 8, 1);
            }
            else
            {
                int firstComma = response.IndexOf(",", start + 1);
                if (firstComma - start < 8 || firstComma == -1)
                    return response; // Don't need to fix.

                try
                {
                    string value = response.Substring(start + 6, firstComma - start - 5);
                    int val = Convert.ToInt32(value);
                }
                catch
                {
                    return response;
                }

                response = response.Remove(start + 8, firstComma - start - 7);
            }

            return response;
        }

        private Action<IList<int>, IList<FriendRequestEntity>> _callback;
    }
}
