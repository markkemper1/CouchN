using System;
using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture]
    public class CamelCaseTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }


        [Test]
        public void serialize_a_string()
        {
            var json = "test".Serialize();
            Console.WriteLine(json);

            var backAgain = json.DeserializeObject<string>();
            Console.WriteLine(backAgain);
        }

    }
}

