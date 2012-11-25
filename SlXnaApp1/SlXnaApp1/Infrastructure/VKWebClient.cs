using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SlXnaApp1.Infrastructure
{
    /// <summary>
    /// Web client.
    /// </summary>
    internal class VKWebClient
    {
        #region Constructor

        public VKWebClient()
        {
            _waitHandle = new ManualResetEvent(true);
        }

        #endregion

        #region Public methods

        public void SendRequest(string url, string query,  Action<string> resultHandler)
        {
            Debug.Assert(url != null);

            _resultHandler = resultHandler;

            try
            {
                // Build request string
                UriBuilder uriBuilder = new UriBuilder(url);
                uriBuilder.Query = query;

                // Create request
                var request = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                request.Method = "GET";

                _waitHandle.Reset();

                request.BeginGetResponse(new AsyncCallback(_GetResponseCallback), request);

                ThreadPool.QueueUserWorkItem(_WaitCallback, request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SendRequest failed: " + ex.Message);
            }
        }

        public void SendPostRequest(string url, string query,  Action<string> resultHandler)
        {
            Debug.Assert(url != null);

            _resultHandler = resultHandler;
            _postData = query;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = REQUEST_TYPE;

                _waitHandle.Reset();

                req.BeginGetRequestStream(new AsyncCallback(_GetRequestStreamCallback), req);

                ThreadPool.QueueUserWorkItem(_WaitCallback, req);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SendPostRequest failed: " + ex.Message);
            }
        }

        public void GetByteData(string url, Action<Stream> byteDataHandler)
        {
            if (url == null || byteDataHandler == null)
                throw new ArgumentNullException("One or more of arguments is null reference!");

            _byteDataHandler = byteDataHandler;

            try
            {
                UriBuilder uriBuilder = new UriBuilder(url);

                var request = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());

                _waitHandle.Reset();

                request.BeginGetResponse(new AsyncCallback(_GetImageResponseCallback), request);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (!_waitHandle.WaitOne(TIMEOUT))
                    {
                        (state as HttpWebRequest).Abort();
                    }
                }, request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetByteData failed: " + ex.Message);
            }
        }

        public void SendPhoto(string url, string parameter, byte[] data, Action<string> resultHandler)
        {
            if (string.IsNullOrEmpty(url) ||
                string.IsNullOrEmpty(parameter) ||
                resultHandler == null)
            {
                throw new ArgumentNullException("One or more of arguments is null reference!");
            }

            _postData = parameter;
            _postBytes = data;
            _resultHandler = resultHandler;

            try
            {
                UriBuilder uriBuilder = new UriBuilder(url);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                _boundary = String.Format(BOUNDARY, Guid.NewGuid());

                req.Method = "POST";
                req.ContentType = String.Format(UPLOAD_TYPE, _boundary);

                _waitHandle.Reset();

                req.BeginGetRequestStream(new AsyncCallback(_GetBytesRequestStreamCallback), req);

                ThreadPool.QueueUserWorkItem(_WaitCallback, req);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetByteData failed: " + ex.Message);
            }
        }

        #endregion

        #region Private methods

        private void _WaitCallback(object state)
        {
            if (!_waitHandle.WaitOne(TIMEOUT))
            {
                (state as HttpWebRequest).Abort();
            }
        }

        private void _GetBytesRequestStreamCallback(IAsyncResult result)
        {
            Stream postStream = null;
            HttpWebRequest request = null;

            try
            {
                request = (HttpWebRequest)result.AsyncState;
                postStream = request.EndGetRequestStream(result);

                byte[] contentFile = Encoding.UTF8.GetBytes(String.Format(TEMP_FILE,
                    _boundary, "photo", "photo.jpg", STREAM_TYPE));
                postStream.Write(contentFile, 0, contentFile.Length);

                postStream.Write(_postBytes, 0, _postBytes.Length);

                byte[] lineFeed = Encoding.UTF8.GetBytes("\r\n");
                postStream.Write(lineFeed, 0, lineFeed.Length);

                byte[] contentEnd = Encoding.UTF8.GetBytes(String.Format(TEMP_END, _boundary));
                postStream.Write(contentEnd, 0, contentEnd.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetBytesRequestStreamCallback failed: " + ex.Message);
            }
            finally
            {
                if (postStream != null)
                    postStream.Close();
            }

            request.BeginGetResponse(new AsyncCallback(_GetResponseCallback), request);
        }

        private void _GetRequestStreamCallback(IAsyncResult result)
        {
            Stream postStream = null;
            HttpWebRequest request = null;

            try
            {
                request = (HttpWebRequest)result.AsyncState;
                postStream = request.EndGetRequestStream(result);
                byte[] data = Encoding.UTF8.GetBytes(_postData);
                postStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetRequestStreamCallback failed: " + ex.Message);
            }
            finally
            {
                if (postStream != null)
                    postStream.Close();
            }

            request.BeginGetResponse(new AsyncCallback(_GetResponseCallback), request);
        }

        private void _GetImageResponseCallback(IAsyncResult result)
        {
            _waitHandle.Set();

            Stream stream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

                stream = response.GetResponseStream();
            }
            catch (WebException ex)
            {
                Debug.WriteLine("The request was aborted: " + ex.Status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("In _GetImageResponseCallback exception occurred: " + ex.Message);
            }
            finally
            {
                _byteDataHandler(stream);
            }
        }

        private void _GetResponseCallback(IAsyncResult result)
        {
            _waitHandle.Set();

            string data = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

                data = _ReadTextContent(response);
            }
            catch (WebException ex)
            {
                Debug.WriteLine("The request was aborted: " + ex.Status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("In GetResponseCallback exception occurred: " + ex.Message);
            }
            finally
            {
                _resultHandler(data);
            }
        }

        private string _ReadTextContent(WebResponse resp)
        {
            string data = null;

            try
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("In _ReadTextContent exception occured: " + ex.Message);
            }

            return data;
        }

        #endregion

        #region Private constants

        private const int  TIMEOUT = 30000;
        private const string TEMP_FILE = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
        private const string TEMP_END = "--{0}--\r\n\r\n";
        private const string BOUNDARY = "--{0}";
        private const string UPLOAD_TYPE = "multipart/form-data; boundary={0}";
        private const string REQUEST_TYPE = "application/x-www-form-urlencoded";
        private const string STREAM_TYPE = "application/octet-stream";

        #endregion

        #region Private fields

        private string _postData;
        private byte[] _postBytes;
        private string _boundary;

        private Action<string> _resultHandler;
        private Action<Stream> _byteDataHandler;
        private ManualResetEvent _waitHandle;

        #endregion
    }
}
