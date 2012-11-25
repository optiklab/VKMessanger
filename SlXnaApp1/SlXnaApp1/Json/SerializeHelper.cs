using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SlXnaApp1.Json
{
    internal static class SerializeHelper
    {
        #region Public methods

        public static T Deserialise<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            T obj = Activator.CreateInstance<T>();

            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(stream);
            }

            return obj;
        }

        public static string FixResponseArrayString(string response)
        {
            if (string.IsNullOrEmpty(response))
                return string.Empty;

            int emptyArray = response.IndexOf("[0]");

            if (emptyArray > -1)
            {
                response = response.Remove(RESPONSE_HEADER_LENGTH, 1);
            }
            else
            {
                int firstComma = response.IndexOf(",");
                if (firstComma < RESPONSE_HEADER_LENGTH || firstComma == -1)
                    return response; // Don't need to fix.

                response = response.Remove(RESPONSE_HEADER_LENGTH, firstComma - (RESPONSE_HEADER_LENGTH - 1));
            }

            return response;
        }

        #endregion

        #region Private constants

        private const int RESPONSE_HEADER_LENGTH = 13;

        #endregion
    }
}
