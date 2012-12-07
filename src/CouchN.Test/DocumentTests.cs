using System;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace CouchN.Test
{
    [TestFixture]
    public class DocumentsTest
    {
        [Test]
        public void db_put_get_delete()
        {
            using (var session = new TemporarySession())
            {
                var testObject = new TestDoc { Text = "hello world" };

                var result = session.Get<TestDoc>("test");

                Assert.That(result, Is.Null);

                var info = session.Save(testObject, "test");

                testObject.Text = "hello world chagned";

                session.Save(testObject);

                testObject.Text = "hello world changed";

                info = session.Save(testObject);

                testObject.Text = "hello world changed 2";

                session.Save(testObject, info);

                session.Delete(testObject);

                result = session.Get<TestDoc>("test");

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void should_throw_an_exception_if_unique_constraint_violated()
        {
            using (var session = new TemporarySession())
            {
                Documents.Configure<TestDocUnique>()
                    .UniqueConstraint = unique => unique.Name;

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                Console.Write(result.Id);

                var conflictDOc = new TestDocUnique { Name = "hello world" };

                Assert.Throws<ArgumentException>(() => session.Documents.Save(conflictDOc));


            }
        }


        [Test]
        public void should_be_able_to_delete_a_documents_and_add_another_with_same_key()
        {
            using (var session = new TemporarySession())
            {
                Documents.Configure<TestDocUnique>()
                    .UniqueConstraint = unique => unique.Name;

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                Console.Write(result.Id);

                session.Delete(testObject);

                var conflictDOc = new TestDocUnique { Name = "hello world" };

                var result2 = session.Documents.Save(conflictDOc);

                Assert.AreNotEqual(result.Id, result2.Id);
            }
        }


        [Test]
        public void should_be_able_to_use_a_slight_of_hand_with_my_types_id()
        {
            using (var session = new TemporarySession())
            {
                var config = Documents.Configure<TestDocUnique>();
                config.CreateId = unique => "TestDocUnique-" + unique.Name;
                config.ResolveId = s => "TestDocUnique-" + s;

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                Console.Write(result.Id);

                var testObjectFromDb = session.Documents.Get<TestDocUnique>("hello world");

                Assert.NotNull(testObjectFromDb);
            }
        }

        [Test]
        public void should_be_able_to_use_a_custom_documet_type()
        {
            using (var session = new TemporarySession())
            {
                var config = Documents.Configure<TestDocUnique>();
                config.TypeName = "testdocunique";

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                Console.Write(result.Id);

                var testObjectFromDb = session.Documents.Get<JObject>(result.Id);

                Assert.AreEqual("testdocunique", testObjectFromDb["type"].ToString());
            }
        }



        [Test]
        public void should_be_able_to_soft_delete_a_document()
        {
            using (var session = new TemporarySession())
            {
                var config = Documents.Configure<TestDocUnique>();
                config.TypeName = "testdocunique";

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                Console.Write(result.Id);

                session.Documents.Delete(testObject);

                Console.WriteLine("x");
            }
        }

        [Test]
        public void should_save_a_document_attachment()
        {
            using (var session = new TemporarySession())
            {
                var config = Documents.Configure<TestDocUnique>();
                config.TypeName = "testdocunique";

                var testObject = new TestDocUnique { Name = "hello world" };

                var result = session.Documents.Save<TestDocUnique>(testObject);

                var data64String = "VGhpcyBpcyBhIGJhc2U2NCBlbmNvZGVkIHRleHQ=";
                var bytes = Convert.FromBase64String(data64String);
                string attachmentName = "testAttachment";

                result = session.Documents.PutAttachment(result.Id,result.Revision, attachmentName, "text/plain", bytes);

                var bytesStored = session.Documents.GetAttachment(result.Id, attachmentName);

                var doc = Convert.ToBase64String(bytesStored);

                Console.WriteLine(Encoding.UTF8.GetString(bytesStored));

                Assert.That(doc, Is.EqualTo(data64String));

                session.Documents.DeleteAttachment(result.Id, result.Revision, attachmentName);

                Assert.That(session.Documents.GetAttachment(result.Id, attachmentName), Is.Null);
            }
        }

        [Test]
        public void should_save_a_document_version_history_when_enabled()
        {
            using (var session = new TemporarySession())
            {
                var config = Documents.Configure<TestDocUniqueVersioned>();
                config.KeepHistory = true;

                var testObject = new TestDocUniqueVersioned { Name = "hello world" };

                var result = session.Documents.Save<TestDocUniqueVersioned>(testObject);

                testObject.Name = "Hello World 2";

                result = session.Documents.Save<TestDocUniqueVersioned>(testObject);

                testObject = session.Documents.Get<TestDocUniqueVersioned>(result.Id);

                Assert.That(testObject._Attachments, Is.Not.Null);
                Assert.That(testObject._Attachments.Count, Is.EqualTo(1));

                testObject.Name = "Rock Out";

                result = session.Documents.Save<TestDocUniqueVersioned>(testObject);
            }
        }

        [Test]
        public void json_debug()
        {
            var json = @"{
  ""content_type"": ""text/json"",
  ""revpos"": 3,
  ""digest"": ""md5-1haOuxvSBg01LzTCRv+FCw=="",
  ""length"": 161,
  ""stub"": true
}";

            var item = JObject.Parse(json);
        }

        //[Test]
        //public void should_be_able_to_soft_delete_a_document()
        //{
        //    using (var session = new TemporarySession())
        //    {
        //        var config = Documents.Configure<TestDocUnique>();
        //        config.TypeName = "testdocunique";

        //        var testObject = new TestDocUnique { Name = "hello world" };

        //        var result = session.Documents.Save<TestDocUnique>(testObject);

        //        Console.Write(result.Id);

        //        session.Documents.Delete(testObject);

        //        Console.WriteLine("x");
        //    }
        //}


        [Test, Ignore]
        public void should_allow_keys_to_contain_slashes()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { _id="test/1", Text = "hello world" };

                var result1 = session.Get<TestDoc>("test/1");

                Assert.That(result1, Is.Null);

                session.Save(testObject1, testObject1._id);

                var result2 = session.Get<TestDoc>("test/1");

                Assert.That(result2, Is.Not.Null);
                Assert.That(result2._id == "Test/1");


                var testObject2 = new TestDoc { _id = "test/2", Text = "hello world" };

                var result3 = session.Get<TestDoc>("test/2");

                Assert.That(result3, Is.Null);

                session.Save(testObject2, testObject2._id);

                var result4 = session.Get<TestDoc>("test/2");

                Assert.That(result4, Is.Not.Null);
                Assert.That(result4._id == "Test/2");
               
            }
        }
    }

    public class TestConvention
    {
        public string Name { get; set; }
    }

    public class TestDocUnique
    {
        public string Name { get; set; }
    }

    public class TestDocUniqueVersioned 
    {
        public string Name { get; set; }
        public AttachmentList _Attachments { get; set; }
    }

    public class TestDoc
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string Text { get; set; }
    }
}
