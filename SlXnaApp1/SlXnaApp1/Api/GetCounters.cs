using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    [DataContract(Name = "response")]
    public class CountersEntity
    {
        [DataMember(Name = "messages")]
        public int MessagesCount { get; set; }
    }

    /// <summary>
    /// Is not used.
    /// </summary>
    public class GetCounters : APIRequest
    {
        public GetCounters(Action<int> callback)
            : base("getCounters")
        {

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                CountersEntity succeed = SerializeHelper.Deserialise<CountersEntity>(response);

                _callback(succeed.MessagesCount);
            }
            catch
            {
                Debug.WriteLine("Parse response from GetCounters failed.");

                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
