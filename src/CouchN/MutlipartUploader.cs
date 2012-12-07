//using System;
//using System.Collections.Specialized;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;

//namespace CouchN
//{
//    public class MuiltpartRelatedHelper : IDisposable
//    {
//        public HttpWebRequest Request { get; set; }
//        private readonly string boundary;
//        public bool ended = false;
//        private StringBuilder debug;
//        private bool first = true;

//        public static MuiltpartRelatedHelper Create(Uri uri, string contentType="multipart/related", string boundary = null)
//        {
//            var request = (HttpWebRequest)WebRequest.Create(uri);
//            return new MuiltpartRelatedHelper(request, contentType, boundary);
//        }

//        public void DebugOn()
//        {
//            debug = new StringBuilder();
//        }
//        public string DebugInfo()
//        {
//            if(debug ==null) throw new InvalidOperationException("Debugging was not turned on");

//            return debug.ToString();
//        }

//        public MuiltpartRelatedHelper(HttpWebRequest request, string contentType = "multipart/related", string boundary = null)
//        {
//            if (request == null) throw new ArgumentNullException("request");
//            this.boundary = boundary ?? "the_multipart_boundary_" + DateTime.UtcNow.Ticks.ToString("x");
//            Request = request;
//            Request.Proxy = new WebProxy(new Uri("http://work:8888", false));
//            Request.ContentType = contentType + @"; boundary=" + this.boundary + @"";
//            Request.Method = "PUT";
//          //  Request.KeepAlive = true;
//           // Request.Credentials = System.Net.CredentialCache.DefaultCredentials;
           
//        }

//        public void AddPart(string content, string contentType = "application/json")
//        {
//            var rs = Request.GetRequestStream();

//            var writer = new StreamWriter(rs, Encoding.ASCII, 1024);

//            writer.Write((first ? "" :"\r\n") + "--" + boundary + "\r\n");
//            first = false;
//            if (debug != null) debug.Append("\r\n--" + boundary + "\r\n");

//            if (contentType != null)
//            {
//                writer.Write("Content-Type: " + contentType + "\r\n");
//                if (debug != null) debug.Append("Content-Type: " + contentType + "\r\n");
//            }

//            writer.Write("\r\n");
//            if (debug != null) debug.Append("\r\n");

//            writer.Write(content);
//            if (debug != null) debug.Append(content);

//            writer.Flush();
//        }

//        private void End()
//        {
//            if (!ended)
//            {
//                var rs = Request.GetRequestStream();
//                var writer = new StreamWriter(rs, Encoding.ASCII, 1024);

//                writer.Write("\r\n--" + boundary + "--\r\n");
//                if (debug != null) debug.Append("\r\n--" + boundary + "--\r\n");
//                writer.Flush();
//                rs.Close();
//            }
//        }

//        public string Execute()
//        {
//            this.End();
//            WebResponse wresp = null;
//            try
//            {
//                Request.Timeout = 15 * 5000;
//                wresp = Request.GetResponse();
//                using (var stream2 = wresp.GetResponseStream())
//                {
//                    var reader2 = new StreamReader(stream2);

//                    var result = reader2.ReadToEnd();
//                    wresp.Close();
//                    return result;
//                }
//            }
//            catch (WebException ex)
//            {
//                if (ex.Response != null)
//                {
//                    using (var stream = ex.Response.GetResponseStream())
//                    {
//                        using (var reader = new StreamReader(stream))
//                        {
//                            throw new ApplicationException(reader.ReadToEnd(), ex);
//                        }
//                    }
                    
//                }

//                wresp = null;
//                throw;
//            }
//            finally
//            {
//                if(wresp != null)
//                     wresp.Close();
//                Request = null;
//            }
//        }


//        public void Dispose()
//        {
            
//        }
//    }

//}