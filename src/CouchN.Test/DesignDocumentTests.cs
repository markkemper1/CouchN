using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture]
    public class DesignDocumentsTest
    {
        [Test]
        public void create_design_doc_with_a_view_and_query()
        {
            using (var session = new TemporarySession())
            {
                var testObject = new TestDoc {Text = "hello world"};

                var info1 = session.Documents.Save(testObject, "test1");
                var info2 = session.Documents.Save(testObject, "test2");


                var designDoc = new SimpleDesignDocument();
                designDoc.Views["test"] = new SimpleDesignDocument.View()
                                              {
                                                  Map = "function(doc){ emit(doc._id); }"
                                              };

                session.Design("test").Put(designDoc);


                var results = session.Design("test").View<object, TestDoc>("test");


                Assert.That(results.Rows.Length, Is.EqualTo(2));


                results = session.Design("test").ViewDocs<TestDoc>("test", track: true);

                var doc1 = results.Documents[0];

                doc1.Text = "arg I got changed from a view result";
                session.Documents.Save(doc1);

            }
        }

        [Test]
        public void create_design_doc_with_a_update_handler_create_new_doc()
        {
            using (var session = new TemporarySession())
            {
                var designDoc = new SimpleDesignDocument();
                designDoc.Updates["test"] = @"
function(doc,req)
{
    if (!doc) {
        if (req.id)
            return [{ _id : req.id}, Text= toJSON('New World')]
        return [null, toJSON('Empty World')];
    }
    doc.world = 'hello';
    doc.edited_by = req.userCtx;
    return [doc, toJSON('Existing World')];
}".Trim();

                session.Design("test").Put(designDoc);

                var testObject = new TestDoc { Text = "hello world" };
                var results = session.Design("test").Update<TestDoc, string>("test", testObject);

                Assert.That(results, Is.EqualTo("New World"));

                results = session.Design("test").Update<TestDoc, string>("test", testObject);

                Assert.That(results, Is.EqualTo("Existing World"));
            }
        }

        public class TestDoc
        {
            public string Text { get; set; }
        }
    }

    
}
