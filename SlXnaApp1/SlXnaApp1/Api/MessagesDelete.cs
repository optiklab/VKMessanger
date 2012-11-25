using System;
using SlXnaApp1.Json;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SlXnaApp1.Api
{
    public class MessagesDelete : APIRequest
    {
        public MessagesDelete(string mids, Action<Dictionary<string, int>> callback)
            : base("messages.delete")
        {
            if (string.IsNullOrEmpty(mids))
            {
                Debug.Assert(false);
            }

            base.AddParameter("mids", mids);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var result = new Dictionary<string, int>();

                var regToText = new Regex("\"((\\.|[^\"])*)", RegexOptions.Singleline);
                Match m = regToText.Match(response);
                while (m.Success)
                {
                    string value = m.Value;

                    try
                    {
                        string text = value.TrimStart('"');
                        text = text.TrimEnd('"');

                        int test = Convert.ToInt32(text); // exception

                        int index = response.IndexOf(value);

                        Char success = response[index + value.Length + 2];

                        if (success == '1')
                            result.Add(text, 1);
                        else
                            result.Add(text, 0);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Parse response with Regex failed: " + ex.Message);
                    }

                    m = m.NextMatch();
                }

                _callback(result);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesDelete failed.");

                _callback(null);
            }
        }

        private Action<Dictionary<string, int>> _callback;
    }
}
