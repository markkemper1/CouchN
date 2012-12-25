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
    }
}
