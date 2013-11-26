using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

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
                var testObject = new TestDoc { Text = "hello world" };

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

                var orginalInfo = session.Documents.GetInfo(testObject);

                results = session.Design("test").Update<TestDoc, string>("test", new TestDoc() { Text = "brand new" }, orginalInfo.Id);

                Assert.That(results, Is.EqualTo("Existing World"));
            }
        }

        [Test, Explicit]
        public void x()
        {
            Documents.Configure<Port>((info, port) => port.Id = info.Id).UniqueConstraint = p => "--" + p.Slug;

            using (var session = new CouchSession(new Uri("http://127.0.0.1:5984"), "cruiseme"))
            {

                var results = session.Design("ports").ViewDocs<Port>("by_slug");

            }
        }

        [Test]
        public void test_update_handlers()
        {
            using (var session = new TemporarySession())
            {
                var designDoc = new SimpleDesignDocument();
                designDoc.Updates["test"] = @"
function(doc,req)
{
   
    var resp = {
        'headers': {
            'Content-Type': 'application/json'
        },
        'body': toJSON( JSON.parse(req.body) )
    };
    
    return [doc, resp];

}".Trim();

                session.Design("test").Put(designDoc);

                var response = session.Design("test").Update<TestDoc, JObject>("test", new TestDoc() { Text = "brand new" }, Guid.NewGuid().ToString());

                Console.WriteLine(response);

            }
        }

        [Test]
        public void test_update_handlers_with_document()
        {
            using (var session = new TemporarySession())
            {
                var designDoc = new SimpleDesignDocument();
                designDoc.Updates["test1"] = @"
function(doc,req)
{
   
    var resp = {
        'headers': {
            'Content-Type': 'application/json'
        },
        'body': req.body ? toJSON( JSON.parse(req.body) ) : toJSON({m: 'null body'})
    };
    
    return [doc, resp];

}".Trim();

                session.Design("test1").Put(designDoc);

                var response = session.Design("test1").Update<JToken>("test1", "x", payload: new {Hello="World"});

                Console.WriteLine(response);

            }
        }



        [Test]
        public void test_filters()
        {
            using (var session = new TemporarySession())
            {
                var designDoc = new SimpleDesignDocument();
                designDoc.Filters["testFilter"] = @"
function(doc,req)
{
   
    return doc.filter;

}".Trim();

                session.Design("test").Put(designDoc);

                session.Documents.Save(new {item="1", filter= false});
                session.Documents.Save(new { item = "2", filter = true });
                session.Documents.Save(new { item = "3", filter = true });

                var results = session.Changes.Poll(new ChangesQuery() {Filter = "test/testFilter"});

                Assert.That(results.Results.Length, Is.EqualTo(2));

                results = session.Changes.Poll(new ChangesQuery() {  });

                Assert.That(results.Results.Length, Is.EqualTo(4));

            }
        }


        public class Port
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Slug { get; set; }

        }

        public class TestDoc
        {
            public string Text { get; set; }
        }
    }


}
