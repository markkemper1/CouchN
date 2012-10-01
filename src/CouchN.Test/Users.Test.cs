using System;
using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture]
    public class Users
    {
        [Test]
        public void should_be_able_to_create_a_user()
        {
            using (var session = new TemporarySession())
            {
                var user = new User()
                               {
                                   Name = Guid.NewGuid().ToString()
                               };

                session.Users.Save(user);

                Console.WriteLine(session.DatabaseName);
            }
        }

        [Test]
        public void should_be_able_to_set_password_and_authenticate()
        {
            using (var session = new TemporarySession())
            {
                var user = new User()
                {
                    Name = Guid.NewGuid().ToString(),

                    Roles = new[] {"jedi"}
                };

                var password = Guid.NewGuid().ToString();

                session.Users.Save(user);

                session.Users.SetPasword(user.Name, password);

                var result = session.Users.Authenticate<User>(user.Name, password);

                Assert.IsTrue(result.OK);
                Assert.That(result.User.Roles[0], Is.EqualTo("jedi"));
            }
        }

        [Test]
        public void should_be_able_to_create_and_get_a_user()
        {
            using (var session = new TemporarySession())
            {
                var user = new User()
                {
                    Name = Guid.NewGuid().ToString()
                };

                session.Users.Save(user);

                var fromDb = session.Users.Get<User>(user.Name);

                Assert.IsNotNull(fromDb);
            }
        }

        [Test]
        public void should_be_able_to_set_password_and_authenticate_2()
        {
            using (var session = new TemporarySession())
            {
                var user = new User()
                {
                    Name = Guid.NewGuid().ToString(),
                };

                var password = Guid.NewGuid().ToString();

                session.Users.Save(user);

                session.Users.SetPasword(user.Name, password);

                var result = session.Users.Authenticate<User>(user.Name, password);

                Assert.IsTrue(result.OK);
            }
        }
    }


    public class User
    {
        public string Name { get; set; }

        public string[] Roles { get; set; }
    }
}
