using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Range = Moq.Range;

namespace UnitTest
{
    public interface IBaz
    {
         string Name { get; set; }
    }

    public class Consumer
    {
        private readonly IFoo _foo;

        public Consumer(IFoo foo)
        {
            _foo = foo;
        }

        public void Hello()
        {
            _foo.DoSomething("ping");
            var name = _foo.Name;
            _foo.SomeOtherProperty = 123;
        }
    }

    public abstract class Person
    {
        protected int SSN { get; set; }

        protected abstract void Execute(string cmd);
    }

    public delegate void AlienAbduction(int galaxy);

    public interface IAnimal
    {
        event EventHandler FallsIll;
        void Stumble();

        event AlienAbduction AlienAbduction;
    }

    public class Doctor
    {
        public int TimesCured { get; set; }
        public int AbductionObserved { get; set; }
        public Doctor(IAnimal animal)
        {
            animal.FallsIll += (sender, args) =>
            {
                TimesCured++;
                Console.WriteLine("I will Cure You");
            };

            animal.AlienAbduction += galaxy => ++AbductionObserved;
        }


    }

    public interface IFoo
    {
        bool DoSomething(string value);
        string ProcessString(string value);
        bool TryParse(string value, out string result);
        bool Submit(ref BankAccount value);
        string Name { get; set; }
        int SomeOtherProperty { get; set; }
        bool Add(int amount);
        IBaz Baz { get; set; }
        int GetCount();
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

        [Test]
        public void ProcessString()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.ProcessString(It.IsAny<string>())).Returns((string s) => s.ToLowerInvariant());

            var calls = 0;

            mock.Setup(foo => foo.GetCount()).Returns(() => calls).Callback(() => ++calls);

            mock.Object.GetCount();
            mock.Object.GetCount();

            Assert.That(mock.Object.ProcessString("ABC") ,Is.EqualTo("abc"));
            Assert.That(calls,Is.EqualTo(2));
        }

        [Test]
        public void TestException()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.DoSomething("kill")).Throws<InvalidOperationException>();

            Assert.Throws<InvalidOperationException>(() => mock.Object.DoSomething("kill"));
        }

        [Test]
        public void PropertyTest()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.Name).Returns("bar");

            Assert.That(mock.Object.Name,Is.EqualTo("bar"));

            mock.Setup(foo => foo.Baz.Name).Returns("Baz");
            Assert.That(mock.Object.Baz.Name,Is.EqualTo("Baz"));

            bool setterCalled = false;

            mock.SetupSet(foo =>
            {
                foo.Name = It.IsAny<string>();

            }).Callback<string>(s =>
            {
                setterCalled = true;
            });

            mock.Object.Name = "def";

            Assert.IsTrue(setterCalled);


            mock.SetupProperty(foo => foo.Name);

            IFoo f = mock.Object;

            f.Name = "abc";

            Assert.That(f.Name,Is.EqualTo("abc"));
        }

        [Test]
        public void TestEvents()
        {
            var mock = new Mock<IAnimal>();

            var doctor = new Doctor(mock.Object);

            mock.Raise(animal => animal.FallsIll+=null,
                new EventArgs());

            mock.Setup(animal => animal.Stumble()).Raises(animal => animal.FallsIll += null,
                new EventArgs());

            mock.Object.Stumble();

            mock.Raise(animal => animal.AlienAbduction+=null,42);

            Assert.That(doctor.TimesCured,Is.EqualTo(2));
            Assert.That(doctor.AbductionObserved,Is.EqualTo(1));
        }

        [Test]
        public void TestCallBacks()
        {
            var mock = new Mock<IFoo>();

            int x = 0;

            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true).Callback<string>((s) => x += s.Length);

            mock.Object.DoSomething("abc");

            Assert.That(x,Is.EqualTo(3));
        }

        [Test]
        public void VerificationTest()
        {
            var mock = new Mock<IFoo>();

            var consumer = new Consumer(mock.Object);

            consumer.Hello(); 

            mock.Verify(foo => foo.DoSomething("ping"),Times.AtLeastOnce);
            mock.Verify(foo=>foo.DoSomething("pong"),Times.Never);

            mock.VerifyGet(foo => foo.Name,Times.Exactly(1));
            mock.VerifySet(foo => foo.SomeOtherProperty,Times.Exactly(1));
        }

        [Test]
        public void ProtectedMemberTest()
        {
            var mock = new Mock<Person>();

            mock.Protected().SetupGet<int>("SSN").Returns(42);

            mock.Protected().Setup<string>("Execute",ItExpr.IsAny<string>());

        }
    }
}
