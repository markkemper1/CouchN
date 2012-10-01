using System;
using System.Linq;
using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture]
    public class CloudantTest
    {
        [Test, Explicit]
        public void test_cloudant_connection()
        {
           using(var couch =
                new CouchSession(
                    new Uri(
                        "https://397ad533-6a4c-4a30-813f-bbf616b6d2c9.appharbor:6jLDgafReC50mf6tcgRmumJy@397ad533-6a4c-4a30-813f-bbf616b6d2c9.appharbor.cloudant.com"),
                    "cruiseme"))
            {
                var dbs = couch.Db.Get();

                Console.WriteLine(dbs);

            }
        }
    }
}
