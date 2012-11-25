using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    class UserInfoComparer : IEqualityComparer<UserInfo>
    {
        public bool Equals(UserInfo x, UserInfo y)
        {
            if (x.Uid == y.Uid)
                return true;
            else
                return false;
        }

        public int GetHashCode(UserInfo obj)
        {
            return -1;
        }
    }

    public class ExecuteDialogs : APIRequest
    {
        public ExecuteDialogs(int offset, Action<IList<Dialog>, IList<UserInfo>> callback)
            : base("execute")
        {
            base.AddParameter("code", "var a=API.messages.getDialogs({count:20,preview_length:30,offset:" + offset.ToString() + "});"+
                                      "var p=API.getProfiles({uids:a@.uid,fields:\"first_name,last_name,contacts,online,photo,photo_medium,photo_big,photo_rec,has_mobile,mobile_phone,home_phone\"});" +
                                      "var p2=API.getProfiles({uids:a@.chat_active,fields:\"first_name,last_name,contacts,online,photo,photo_medium,photo_big,photo_rec,has_mobile,mobile_phone,home_phone\"});" +
                                      "return{a:a,p:p,p2:p2};");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(new List<Dialog>(), new List<UserInfo>());
                else
                {
                    // Remove number from array "a"
                    response = _FixResponseArrayString(response);
                    // Remove "false" from "p" and "p2"
                    response = _FixResponseProfilesString(response);

                    var messagesResponse = SerializeHelper.Deserialise<InitializationResponse>(response);

                    List<Dialog> dialogs = new List<Dialog>();
                    List<UserInfo> profiles = new List<UserInfo>();

                    if (messagesResponse.InitializationRResponse.Messages != null)
                    {
                        if (messagesResponse.InitializationRResponse.Profiles != null)
                            profiles.AddRange(messagesResponse.InitializationRResponse.Profiles);

                        if (messagesResponse.InitializationRResponse.ChatProfiles != null)
                            profiles.AddRange(messagesResponse.InitializationRResponse.ChatProfiles);

                        foreach (var message in messagesResponse.InitializationRResponse.Messages)
                        {
                            if (message != null)
                                dialogs.Add(new Dialog(message, profiles));
                        }
                    }

                    _callback(dialogs, profiles.Distinct(new UserInfoComparer()).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from ExecuteDialogs failed." + ex.Message + "\n" + ex.StackTrace);
                _callback(new List<Dialog>(), new List<UserInfo>());
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
            int index = response.IndexOf(",\"p\":false");
            if (index > -1)
            {
                response = response.Remove(index, 10);
            }

            index = response.IndexOf(",\"p2\":false");
            if (index > -1)
            {
                response = response.Remove(index, 11);
            }

            return response;
        }

        #region Private fields

        private const int DIALOGS_COUNT = 20; // Maximum on VK 200

        private Action<IList<Dialog>, IList<UserInfo>> _callback;

        #endregion
    }
}
