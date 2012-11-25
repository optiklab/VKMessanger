using System;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// If user id is in the list of requested friends: adds user to current user friends.
    /// Otherwise creates request to friends.
    /// </summary>
    public class FriendsAdd : APIRequest
    {
        /// <param name="uid">Required.</param>
        /// <param name="text">Not required.</param>
        /// <param name="callback"></param>
        public FriendsAdd(int uid, string text, Action<int> callback)
            : base("friends.add")
        {
            base.AddParameter("uid", uid.ToString());
            base.AddParameter("text", text);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                // 1 - request sent
                // 2 - request accepted
                // 4 - request sent second time
                SuccessEntity code = SerializeHelper.Deserialise<SuccessEntity>(response);

                _callback(code.Succeed);
            }
            catch (Exception)
            {
                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
