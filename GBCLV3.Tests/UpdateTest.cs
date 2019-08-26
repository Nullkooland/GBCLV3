using System;
using System.Diagnostics;
using GBCLV3.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class UpdateTest
    {
        private readonly UpdateService _updateService;

        public UpdateTest()
        {
            _updateService = new UpdateService();
        }

        [TestMethod]
        public void GetUpdateInfoTest()
        {
            var info = _updateService.Check().Result;

            if (info == null)
            {
                Debug.WriteLine("New Version Not Found");
            }
            else
            {
                Debug.WriteLine("New Version Found");

                Debug.WriteLine($"Name:         {info.Name}");
                Debug.WriteLine($"Version:      {info.Version}");
                Debug.WriteLine($"PreRelease:   {info.PreRelease}");
                Debug.WriteLine($"ReleaseTime:  {info.Description}");
            }
        }
    }
}
