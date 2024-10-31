namespace Betsson.OnlineWallets.ApiTests;

using System.Diagnostics;
using NUnit.Framework;

public class WalletAPITests
{
    private WalletServiceClient _client;

    [SetUp]
    public void Setup()
    {
        _client = new WalletServiceClient("http://localhost:5001");
    }

    [Test]
    public void GetBalance_ShouldReturnBalance()
    {
        var balance = _client.GetBalance();
        Assert.IsNotNull(balance.Amount);
    }

    [Test]
    public void Deposit_ShouldIncreaseBalance()
    {
        int amountToDeposit = 100;

        var initialBalance = _client.GetBalance();
        var depositResponse = _client.Deposit(amountToDeposit);
            
        Assert.IsNotNull(depositResponse);
        Assert.IsTrue(depositResponse.Amount == initialBalance.Amount + amountToDeposit, "Balance did not increase after depositing money");
    }

    [Test]
    public void Withdraw_ShouldDecreaseBalance()
    {
        int amountToWithdraw = 50;

        _client.Deposit(100); // Ensure sufficient balance
        var initialBalance = _client.GetBalance();
        var withdrawResponse = _client.Withdraw(amountToWithdraw);
            
        Assert.IsNotNull(withdrawResponse);
        Assert.IsTrue(withdrawResponse.Amount == initialBalance.Amount - amountToWithdraw, "Balance did not decrease after withdrawing money");
    }

    [Test]
    public void Deposit_NegativeAmount_ShouldNotChangeBalance()
    {
        int negativeAmount = -50;
        var initialBalance = _client.GetBalance();
        
        Assert.Throws<HttpRequestException>(() => _client.Deposit(negativeAmount), "Negative deposits should not be allowed.");
        
        var currentBalance = _client.GetBalance();
        Assert.That(currentBalance.Amount, Is.EqualTo(initialBalance.Amount), "Balance should remain the same after attempting a negative deposit.");
    }

    [Test]
    public void Withdraw_InsufficientBalance_ShouldNotChangeBalance()
    {
        var balance = _client.GetBalance();
        int overdraftAmount = balance.Amount + 50;
        
        Assert.Throws<HttpRequestException>(() => _client.Withdraw(overdraftAmount), "Withdrawing more than the balance should not be allowed.");
        
        var currentBalance = _client.GetBalance();
        Assert.That(currentBalance.Amount, Is.EqualTo(balance.Amount), "Balance should remain the same after attempting to overdraft.");
    }
}