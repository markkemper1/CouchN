using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace CouchN.Test
{
    [TestFixture]
    public class ChangesTest
    {
        [Test]
        public void DeserializeObjectTest()
        {
            var content = @"
{""results"":[
{""seq"":3,""id"":""54627b756fe11d9cbd7844f58b444f10"",""changes"":[{""rev"":""1-168d833fd65765b12a548c224417f8e2""}]},
{""seq"":4,""id"":""54627b756fe11d9cbd7844f58b44535f"",""changes"":[{""rev"":""1-5712d582b649f62d732823df8e78e901""}]}
],
""last_seq"":4}
";
            var result = content.DeserializeObject<ChangesResult>();

            Console.WriteLine(SerializerHelper.Serialize(result));

            result.LastSequence.ShouldBe(4);
            result.Results.Length.ShouldBe(2);
            result.Results[0].Sequence.ShouldBe(3);
            result.Results[0].Id.ShouldBe("54627b756fe11d9cbd7844f58b444f10");
            result.Results[0].Changes.Length.ShouldBe(1);
            result.Results[0].Changes[0].Revision.ShouldBe("1-168d833fd65765b12a548c224417f8e2");


        }

        [Test]
        public void TestNormalPoll()
        {
            using (var session = new TemporarySession())
            {
                session.Documents.Save(new { item = "1", filter = false });
                session.Documents.Save(new { item = "2", filter = true });
                session.Documents.Save(new { item = "3", filter = true });

                var results = session.Changes.Poll(new ChangesQuery() { IncludeDocs = true});

                Assert.That(results.Results.Length, Is.EqualTo(3));

            }
        }

        [Test, Explicit]
        public void TestLongPoll()
        {
            using (var session = new TemporarySession())
            {
                session.Documents.Save(new { item = "1", filter = false });
                session.Documents.Save(new { item = "2", filter = true });
                session.Documents.Save(new { item = "3", filter = true });

                Console.WriteLine("YOU NEED TO CHANGE 1 DOCUMENT FO THIS TEST TO WORK, DB: " + session.DatabaseName);
                var results = session.Changes.LongPoll();

                results = session.Changes.LongPoll(new ChangesQuery() { Since = results.LastSequence });

                Assert.That(results.Results.Length, Is.EqualTo(1));

            }
        }

        [Test, Explicit]
        public void TestContinous()
        {
            using (var session = new TemporarySession())
            {
                var run = true;
                using (var task = Task.Run(() =>
                {
                    int id = 1;
                    while (run)
                    {

                        session.Documents.Save(new { item = "item_" + (id++), filter = false });
                        Thread.Sleep(500);
                    }
                }))
                {
                    Console.WriteLine("DB: " + session.DatabaseName);

                    Console.WriteLine(""); Console.WriteLine("");

                    Thread.Sleep(1000);


                    session.Changes.Stream(Console.WriteLine, null, pair => Console.WriteLine("Header: {0}:{1}", pair.Key, pair.Value));


                    run = false;

                    Thread.Sleep(700);


                }


            }
        }
    }
}
