using System;
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
    }

    public class TestConvention
    {
        public string Name { get; set; }
    }

    public class TestDocUnique
    {
        public string Name { get; set; }
    }

    public class TestDoc
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string Text { get; set; }
    }
}
