using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture]
    public class CreateDatabaseTest
    {
        [Test]
        public void db_put_get_delete()
        {
            using (var session = new TemporarySession())
            {
                var db = session.Db.Get();

                Assert.That(db, Is.Not.Null);
                Assert.That(db.Name, Is.EqualTo(session.DatabaseName));

                session.Db.Delete();

                db = session.Db.Get();

                Assert.That(db, Is.Null);
            }
        }

    }
}
