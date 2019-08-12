using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using GBCLV3.Services.Launcher;

namespace GBCLV3.Tests
{
    [TestClass]
    public class AuthTest
    {
        [TestMethod]
        public void LoginTest()
        {
            var authResult = AuthService.LoginAsync("goose_bomb", "123456").Result;

            if (!authResult.IsSuccessful)
            {
                Debug.WriteLine(authResult.ErrorMessage);
            }
        }

        [TestMethod]
        public void RefreshTest()
        {
            var authResult = AuthService.RefreshAsync("233", "2333").Result;

            if (!authResult.IsSuccessful)
            {
                Debug.WriteLine(authResult.ErrorMessage);
            }
        }
    }
}
