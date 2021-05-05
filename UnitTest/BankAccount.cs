using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpromptuInterface;
using Moq;
using NUnit.Framework;

namespace UnitTest
{
    public interface ILog
    {
        void WriteMessage(string msg);
    }

    public class ConsoleLog : ILog
    {
        public void WriteMessage(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    public class Null<T> : DynamicObject where T:class
    {

        public static T Instance => new Null<T>().ActLike<T>();

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            result = default;

            if (typeof(T).GetMethod(binder.Name)?.ReturnType == typeof(void))
                return true;

            Activator.CreateInstance(typeof(T).GetMethod(binder.Name)?.ReturnType??null);
            return true;
        }
    }

   public class BankAccount
    {
        public int Balance { get; set; }
        private readonly ILog _log;
        public BankAccount(int balance, ILog log)
        {
            Balance = balance;
            _log = log;
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Deposit Amount Must Be Positive");

            _log.WriteMessage("Depositing...");
            Balance += amount;
        }

        public bool Withdraw(int amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }

    }


    [TestFixture]
   public class BankAccountTest
   {
       private BankAccount ba;


       [SetUp]
       public void SetUp()
       {
           var log = new Mock<ILog>();
           ba = new BankAccount(50,log.Object );
       }


       [Test]
       
        public void BankAccountShouldIncreaseOnPositiveDeposit()
        {

            ba.Deposit(100);

            Assert.That(ba.Balance,Is.EqualTo(150));
        }


        [Test]
        public void WarningTests()
        {
            Warn.If(2+2 !=5);
            Warn.If(2+2,Is.Not.EqualTo(5));
            Warn.If(()=>2+2,Is.Not.EqualTo(5).After(2000));

            Warn.Unless(2+2==5);
        }


        [Test]
        public void BankAccountShouldThrowUnPositiveAmount()
        {

           var ex= Assert.Throws<ArgumentException>((() => ba.Deposit(-1)));

           StringAssert.StartsWith("Deposit Amount Must Be Positive",ex.Message);
        }

        [Test]
        [TestCase(10,true,40)]
        [TestCase(100,false,50)]
        public void TestMultipleWithdrawScenario(int amount,bool shouldSucceed,int expectedBalance)
        {
            var result = ba.Withdraw(amount);
            
            Assert.Multiple(() =>
            {
                Assert.That(result,Is.EqualTo(shouldSucceed));
                Assert.That(ba.Balance,Is.EqualTo(expectedBalance));
            });
        }

        [Test]
        public void DepositIntegrationTest()
        {
            ba.Deposit(100);

            Assert.That(ba.Balance,Is.EqualTo(150));
        }

       
    }
}
