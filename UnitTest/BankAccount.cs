using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnitTest
{
   public class BankAccount
    {
        public int Balance { get; set; }

        public BankAccount(int balance)
        {
            Balance = balance;
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Deposit Amount Must Be Positive");

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
           ba = new BankAccount(50);
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
   }
}
