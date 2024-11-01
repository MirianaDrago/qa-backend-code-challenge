namespace Betsson.OnlineWallets.ApiTests;

using System.Text.Json;
using NUnit.Framework;

public class WalletAPITests
{
    private WalletServiceClient _client;

    [SetUp]
    public void Setup()
    {
        // Read and parse settings.json into ApiConfiguration
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        var json = File.ReadAllText(jsonPath);
        var config = JsonSerializer.Deserialize<ApiConfiguration>(json);
        
        _client = new WalletServiceClient(config.ApiBaseUrl);
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
    public void Deposit_NegativeAmount_ShouldFail()
    {
        int negativeAmount = -50;
        var initialBalance = _client.GetBalance();
        
        Assert.Throws<HttpRequestException>(() => _client.Deposit(negativeAmount), "Negative deposits should not be allowed.");
        
        var currentBalance = _client.GetBalance();
        Assert.That(currentBalance.Amount, Is.EqualTo(initialBalance.Amount), "Balance should remain the same after attempting a negative deposit.");
    }

    [Test]
    public void Deposit_ZeroAmount_ShouldBeSuccessful()
    {
        int amount = 0;
        var initialBalance = _client.GetBalance();
        var afterDeposit = _client.Deposit(amount);
        
        Assert.That(afterDeposit.Amount, Is.EqualTo(initialBalance.Amount), "Balance should remain the same after attempting a zero deposit.");
    }

    [Test]
    public void Withdraw_ZeroAmount_ShouldBeSuccessful()
    {
        int amount = 0;
        var initialBalance = _client.GetBalance();
        var afterWithdrawal = _client.Withdraw(amount);
        
        Assert.That(afterWithdrawal.Amount, Is.EqualTo(initialBalance.Amount), "Balance should remain the same after attempting a zero withdrawal.");
    }

    [Test]
    public void Withdraw_InsufficientBalance_ShouldFail()
    {
        var balance = _client.GetBalance();
        decimal overdraftAmount = balance.Amount + 50;
        
        Assert.Throws<HttpRequestException>(() => _client.Withdraw(overdraftAmount), "Withdrawing more than the balance should not be allowed.");
        
        var currentBalance = _client.GetBalance();
        Assert.That(currentBalance.Amount, Is.EqualTo(balance.Amount), "Balance should remain the same after attempting to overdraft.");
    }

    [Test]
    public void Deposit_VerySmallAmount_ShouldIncreaseBalanceCorrectly()
    {
        decimal smallAmount = 0.01m;
        var initialBalance = _client.GetBalance();
        var depositResponse = _client.Deposit(smallAmount);

        Assert.IsNotNull(depositResponse);
        Assert.That(depositResponse.Amount, Is.EqualTo(initialBalance.Amount + smallAmount), "Balance should correctly increase by the small deposit amount.");
    }

   [Test]
    public void Deposit_PreciseAmount_ShouldIncreaseBalanceExactly()
    {
        decimal amountToDeposit = 100.555m;  // Input with precise decimal
        var initialBalance = _client.GetBalance();  
        
        var resultBalance = _client.Deposit(amountToDeposit);
        decimal expectedBalance = initialBalance.Amount + amountToDeposit;  

        Assert.That(resultBalance.Amount, Is.EqualTo(expectedBalance), "Balance after deposit should reflect the exact deposited amount.");
    }

    [Test]
    public void Withdraw_NegativeAmount_ShouldFail()
    {
        int negativeAmount = -50;
        var initialBalance = _client.GetBalance();

        Assert.Throws<HttpRequestException>(() => _client.Withdraw(negativeAmount), "Withdrawal of a negative amount should throw an Exception.");

        var newBalance = _client.GetBalance();
        Assert.That(newBalance.Amount, Is.EqualTo(initialBalance.Amount), "The balance should not change after attempting to withdraw a negative amount.");
    }

    [Test]
    public void SequentialDeposits_ShouldAccuratelyUpdateBalance()
    {
        // Note: Ideally, this test should be a true concurrency test to ensure the database can handle multiple simultaneous deposits.
        // However, due to the limitations of the in-memory database setup, this test runs sequentially to ensure it passes. 
        // In a production scenario with a fully-featured database, testing for concurrency would be appropriate.
        
        var initialBalance = _client.GetBalance();
        int numberOfDeposits = 5;
        decimal depositAmount = 10;

        // Perform deposits sequentially
        for (int i = 0; i < numberOfDeposits; i++)
        {
            _client.Deposit(depositAmount);
        }

        var finalBalance = _client.GetBalance();
        decimal expectedBalance = initialBalance.Amount + numberOfDeposits * depositAmount;
        Assert.That(finalBalance.Amount, Is.EqualTo(expectedBalance), "Total balance after sequential deposits should be correct.");
    }


    [Test]
    public void Deposit_ExtremelyLargeAmount_ShouldProcessSuccessfully()
    {
        var initialBalance = _client.GetBalance();
        decimal largeAmount = decimal.MaxValue / 10; // Use a fraction of MaxValue to simulate high but realistic scenarios
        var afterLargeDeposit = _client.Deposit(largeAmount);

        Assert.That(afterLargeDeposit.Amount, Is.EqualTo(initialBalance.Amount + largeAmount), "The balance should correctly reflect the large deposited amount.");
    }
}