using System;
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

        [Test, Explicit]
        public void get_known_db()
        {
            var url =
                "https://397ad533-6a4c-4a30-813f-bbf616b6d2c9.appharbor:6jLDgafReC50mf6tcgRmumJy@397ad533-6a4c-4a30-813f-bbf616b6d2c9.appharbor.cloudant.com";
            using (var session = new CouchSession(new Uri(url), "cruiseme"))
            {
                var db = session.Db.Get();

                Assert.That(db, Is.Not.Null);
                Assert.That(db.Name, Is.EqualTo(session.DatabaseName));
            }
        }

    }
}
