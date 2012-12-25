using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace CouchN.Test
{
    [TestFixture]
    public class BulkDocumentsTest
    {
        [Test]
        public void should_be_able_to_get_all_docs()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { Text = "hello world" };
                var testObject2 = new TestDoc { Text = "hello world" };
                var testObject3 = new TestDoc { Text = "hello world" };

                var info1 = session.Save(testObject1);
                var info2 = session.Save(testObject2);
                var info3 = session.Save(testObject3);

                var result = session.Bulk.All();

                result.Rows.Length.ShouldBe(3);

            }
        }

        [Test]
        public void should_be_able_to_get_delete_all_docs()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { Text = "hello world" };
                var testObject2 = new TestDoc { Text = "hello world" };
                var testObject3 = new TestDoc { Text = "hello world" };

                var info1 = session.Save(testObject1);
                var info2 = session.Save(testObject2);
                var info3 = session.Save(testObject3);

                var result = session.Bulk.All(new ViewQuery(){IncludeDocs = true});
                result.Rows.Length.ShouldBe(3);

                foreach (var document in result.Documents)
                    document["_deleted"] = true;


                session.Bulk.Update(result.Documents.ToArray());

                result = session.Bulk.All(new ViewQuery() { IncludeDocs = true });
                result.Rows.Length.ShouldBe(0);

            }
        }

        [Test]
        public void should_be_able_to_get_delete_all_docs_using_helper()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { Text = "hello world" };
                var testObject2 = new TestDoc { Text = "hello world" };
                var testObject3 = new TestDoc { Text = "hello world" };

                var info1 = session.Save(testObject1);
                var info2 = session.Save(testObject2);
                var info3 = session.Save(testObject3);

                var result = session.Bulk.All(new ViewQuery() { IncludeDocs = true });
                result.Rows.Length.ShouldBe(3);

                session.Bulk.Delete(result.Documents.ToArray());

                result = session.Bulk.All(new ViewQuery() { IncludeDocs = true });
                result.Rows.Length.ShouldBe(0);

            }
        }

        [Test]
        public void should_be_able_insert_typed_documents()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { Text = "hello world 1" };
                var testObject2 = new TestDoc { Text = "hello world 2" };
                var testObject3 = new TestDoc { Text = "hello world 3" };

             

                session.Bulk.Update(new[] {testObject1, testObject2, testObject3});

                var result = session.Bulk.All(new ViewQuery() { IncludeDocs = true });

                result.Rows.Length.ShouldBe(3);
                result.Documents.First().ToObject<TestDoc>().Text.ShouldBe("hello world 1");
            }
        }


        [Test]
        public void should_be_able_update_typed_documents()
        {
            using (var session = new TemporarySession())
            {
                var testObject1 = new TestDoc { Text = "hello world 1" };
                var testObject2 = new TestDoc { Text = "hello world 2" };
                var testObject3 = new TestDoc { Text = "hello world 3" };

                session.Documents.Save(testObject1);
                session.Documents.Save(testObject2);
                session.Documents.Save(testObject3);


                testObject1.Text = "updated 1";
                testObject2.Text = "updated 2";
                testObject3.Text = "updated 3";

                session.Bulk.Update(new[] { testObject1, testObject2, testObject3 });

                var result = session.Bulk.All(new ViewQuery() { IncludeDocs = true });

                result.Rows.Length.ShouldBe(3);
                result.Documents.Skip(0).First()["type"].ShouldBe("TestDoc");
                result.Documents.Skip(0).First().ToObject<TestDoc>().Text.ShouldBe("updated 1");
                result.Documents.Skip(1).First().ToObject<TestDoc>().Text.ShouldBe("updated 2");
                result.Documents.Skip(2).First().ToObject<TestDoc>().Text.ShouldBe("updated 3");
            }
        }
    }
}
