using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Westwind.Utilities.Tests
{
    [TestClass]
    public class ReflectionUtilsTests
    {
        [TestMethod]
        public void TypedValueToStringTest()
        {
            // Guid
            object val = Guid.NewGuid();            
            string res = ReflectionUtils.TypedValueToString(val);

            Assert.IsTrue(res.Contains("-"));
            Console.WriteLine(res);

            object val2 = ReflectionUtils.StringToTypedValue<Guid>(res);
            Assert.AreEqual(val, val2);

            // Single 
            val = (Single) 10.342F;            
            res = ReflectionUtils.TypedValueToString(val);
            Console.WriteLine(res);

            Assert.AreEqual(res, val.ToString());

            val2 = ReflectionUtils.StringToTypedValue<Single>(res);
            Assert.AreEqual(val, val2);

            // Single 
            val = (Single)10.342F;
            res = ReflectionUtils.TypedValueToString(val);
            Console.WriteLine(res);

            Assert.AreEqual(res, val.ToString());

            val2 = ReflectionUtils.StringToTypedValue<Single>(res);
            Assert.AreEqual(val, val2);
        }

        //[TestMethod]
        //public void ComAccessReflectionCoreAnd45Test()
        //{
        //    // this works with both .NET 4.5+ and .NET Core 2.0+

        //    string progId = "InternetExplorer.Application";
        //    Type type = Type.GetTypeFromProgID(progId);
        //    object inst = Activator.CreateInstance(type);


        //    inst.GetType().InvokeMember("Visible", ReflectionUtils.MemberAccess | BindingFlags.SetProperty, null, inst,
        //        new object[1]
        //        {
        //            true
        //        });

        //    inst.GetType().InvokeMember("Navigate", ReflectionUtils.MemberAccess | BindingFlags.InvokeMethod, null,
        //        inst, new object[]
        //        {
        //            "https://markdownmonster.west-wind.com",
        //        });

        //    //result = ReflectionUtils.GetPropertyCom(inst, "cAppStartPath");
        //    bool result = (bool)inst.GetType().InvokeMember("Visible",
        //        ReflectionUtils.MemberAccess | BindingFlags.GetProperty, null, inst, null);
        //    Console.WriteLine(result); // path             
        //}

        //[TestMethod]
        //public void ComAccessDynamicCoreAnd45Test()
        //{
        //    // this does not work with .NET Core 2.0

        //    string progId = "InternetExplorer.Application";
        //    Type type = Type.GetTypeFromProgID(progId);

        //    dynamic inst = Activator.CreateInstance(type);

        //    inst.Visible = true;
        //    inst.Navigate("https://markdownmonster.west-wind.com");

        //    bool result = inst.Visible;
        //    Assert.IsTrue(result);
        //}
    }
}
