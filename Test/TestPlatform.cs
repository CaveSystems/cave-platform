using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Cave;

namespace Test
{
    [TestFixture]
    class TestPlatform
    {
        [Test]
        public void Test()
        {
            Console.WriteLine($"Platform.IsAndroid: {Platform.IsAndroid}");
            Console.WriteLine($"Platform.IsMicrosoft: {Platform.IsMicrosoft}");
            Console.WriteLine($"Platform.IsMono: {Platform.IsMono}");
            Console.WriteLine($"Platform.Type: {Platform.Type}");
            Console.WriteLine($"Platform.SystemVersionString: {Platform.SystemVersionString}");
        }
    }
}
