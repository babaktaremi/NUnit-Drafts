using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Range = Moq.Range;

namespace UnitTest
{
    public interface IFoo
    {
        bool DoSomething(string value);
        string ProcessString(string value);
        bool TryParse(string value, out string result);
        bool Submit(ref BankAccount value);
        string Name { get; set; }
        int SomeOtherProperty { get; set; }
        bool Add(int amount);
    }



    [TestFixture]
   public class MockTest
    {

        [Test]
        public void OrdinaryMethodCalls()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.DoSomething(It.IsIn("Ping","Foo"))).Returns(false);
            mock.Setup(foo => foo.DoSomething("Pong")).Returns(true);

            Assert.Multiple(() =>
            {
                Assert.False(mock.Object.DoSomething("Ping"));
                Assert.False(mock.Object.DoSomething("Foo"));
                Assert.True(mock.Object.DoSomething("Pong"));
            });
        }

        [Test]
        public void ArgumentDependentMatching()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.Add(It.Is<int>(i =>i %2==0 ))).Returns(true);

            mock.Setup(foo => foo.Add(It.IsInRange(1, 10, Range.Inclusive))).Returns(false);

            mock.Setup(foo => foo.DoSomething(It.IsRegex("[a-z]+"))).Returns(false);

           var result= mock.Object.DoSomething("123");
           var result2 = mock.Object.Add(44);

           Assert.Multiple(() =>
           {
               Assert.IsTrue(result2);
               Assert.IsFalse(result);
           });
        }


        [Test]
        public void OutAndRefTest()
        {
            var mock = new Mock<IFoo>();

            var requiredOutput = "ok";

            mock.Setup(foo => foo.TryParse("ping", out requiredOutput)).Returns(true);

            string result;

            Assert.Multiple(() =>
            {
                Assert.IsTrue(mock.Object.TryParse("ping",out result));
                Assert.That(result,Is.EqualTo(requiredOutput));

                var thisShouldBeFalse = mock.Object.TryParse("pong", out result);

                Console.WriteLine(result);
            });

            var account = new BankAccount(100, new ConsoleLog());

            mock.Setup(foo => foo.Submit(ref account)).Returns(true);

            var account2 = new BankAccount(100, new ConsoleLog());

            Assert.That(mock.Object.Submit(ref account),Is.EqualTo(true));

            Assert.That(mock.Object.Submit(ref account2),Is.EqualTo(false));
        }
    }
}
