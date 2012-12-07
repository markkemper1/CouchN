//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using Newtonsoft.Json.Linq;

//namespace CouchN.Test
//{
//    [TestFixture]
//    public class MultipartHelperTest
//    {
//        [Test]
//        public void Test()
//        {
//            using (var session = TheCouch.CreateSession("multipart", new Uri("http://www.localhost.com:5984/")))
//            {
//                //if(session.Db.Get() == null)
//                //    session.Db.Create();

//                var doc = new MultipartTestDoc()
//                    {body = "this is a body."};

//                doc._attachments = new AttachmentList()
//                                       {
//                                           {
//                                               "foo.txt", new Attachment()
//                                                              {
//                                                                  ContentType = "text/plain",
//                                                                  Length = 21,
//                                                                  Follows = true
//                                                              }
//                                           },
//                                           {
//                                               "bar.txt", new Attachment()
//                                                              {
//                                                                  ContentType = "text/plain",
//                                                                  Length = 20,
//                                                                  Follows = true
//                                                              }
//                                           },
//                                       };
//                string id = "multipart";
//                //var exsting = session.Get<MultipartTestDoc>(id);

//                //if(exsting != null)
//                //    session.Delete(exsting);

//                var helper = MuiltpartRelatedHelper.Create(session.GetUri(id, null), "multipart/related", "abc123");

//                helper.AddPart(JObject.FromObject(doc).ToString().Replace("\r\n", "").Replace("\n","").Replace("\r","") );
//                helper.AddPart("this is 21 chars long", null);
//                helper.AddPart("this is 20 chars lon", null);

//var response =                helper.Execute();

//                Console.WriteLine(response);

//            }
//        }
//    }

//    public class MultipartTestDoc
//    {
//        public string body { get; set; }
//        public AttachmentList _attachments { get; set; }
//    }
//}
