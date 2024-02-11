using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Westwind.Utilities.Data.Security;

#if true // disabled for now - config not set up. You can manually remove and set the connection string


namespace Westwind.Utilities.Test
{
    [TestClass]
    public class UserTokenManagerTests
    {
        public const string ConnectionString = "server=.;database=WestwindWebStore;integrated security=true;encrypt=false";

        [TestMethod]
        public void CreateTokenTest()
        {
            var manager = new UserTokenManager(ConnectionString);
            var token = manager.CreateNewToken("1111", "Reference #1", "12345678");

            Assert.IsNotNull(token, manager.ErrorMessage);
            Console.WriteLine(token);
        }

        [TestMethod]
        public void ValidateTokenTest()
        {
            var manager = new UserTokenManager(ConnectionString);
            var token = manager.CreateNewToken("2222", "Reference #1", "12345678");

            Assert.IsNotNull(token, manager.ErrorMessage);
            Console.WriteLine(token);


            Assert.IsTrue(manager.IsTokenValid(token, false), manager.ErrorMessage);
        }

        [TestMethod]
        public void GetTokenByTokenIdentifierTest()
        {
            var tokenIdentifier = DataUtils.GenerateUniqueId();

            var manager = new UserTokenManager(ConnectionString);
            var token = manager.CreateNewToken("3333", "Reference #3", tokenIdentifier);

            Assert.IsNotNull(token, manager.ErrorMessage);
            Console.WriteLine(token);

            var userToken = manager.GetTokenByTokenIdentifier(tokenIdentifier);
            Assert.IsNotNull(userToken, manager.ErrorMessage);
            Console.WriteLine(JsonSerializationUtils.Serialize(userToken));
        }

    }
}

#endif
