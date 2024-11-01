using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Exceptions;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System;

namespace Betsson.OnlineWallets.Tests;

public class Tests
{

    [Test]
    public async Task GetBalanceAsync_ReturnsCorrectBalance_WhenEntriesExist()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        // Mocking the online wallet repository 
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 100, Amount = 50 });
        
        var service = new OnlineWalletService(mockRepo.Object);
        var result = await service.GetBalanceAsync();
        
        Assert.AreEqual(150, result.Amount);
    }

    [Test]
    public async Task GetBalanceAsync_ReturnsZero_WhenNoEntriesExist()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync((OnlineWalletEntry)null); // Returns empty entry (0)
        
        var service = new OnlineWalletService(mockRepo.Object);
        var result = await service.GetBalanceAsync();
        
        Assert.AreEqual(0, result.Amount);
    }

    [Test]
    public async Task DepositFundsAsync_UpdatesBalanceCorrectly_WhenDepositIsMade()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 100, Amount = 50 }); // Returns online wallet entry
        
        mockRepo.Setup(repo => repo.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())) // Accepts any argument
                .Returns(Task.CompletedTask); // Mocks method to immediately return a completed task

        var service = new OnlineWalletService(mockRepo.Object);
        var deposit = new Deposit { Amount = 100 };
        var result = await service.DepositFundsAsync(deposit);
        
        Assert.AreEqual(250, result.Amount); 
    }

    [Test]
    public void WithdrawFundsAsync_ThrowsInsufficientBalanceException_WhenAmountExceedsBalance()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 100, Amount = 50 }); // Latest entry was 150

        var service = new OnlineWalletService(mockRepo.Object);
        var withdrawal = new Withdrawal { Amount = 200 };

        // Should throw InsufficientBalanceException
        Assert.ThrowsAsync<InsufficientBalanceException>(() => service.WithdrawFundsAsync(withdrawal));
    }

    [Test]
    public async Task WithdrawFundsAsync_DecreasesBalanceCorrectly_WhenWithdrawalIsMade()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 200, Amount = 50 }); // Latest entry is 250

        mockRepo.Setup(repo => repo.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

        var service = new OnlineWalletService(mockRepo.Object);
        var withdrawal = new Withdrawal { Amount = 100 };
        var result = await service.WithdrawFundsAsync(withdrawal);
        
        Assert.AreEqual(150, result.Amount); 
    }

    [Test]
    public void DepositFundsAsync_HandlesRepositoryExceptions()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .Throws(new Exception("Database error"));  // Simulate a database failure

        var service = new OnlineWalletService(mockRepo.Object);
        var deposit = new Deposit { Amount = 50 };

        Assert.ThrowsAsync<Exception>(() => service.DepositFundsAsync(deposit));
    }

    [Test]
    public async Task DepositFundsAsync_AllowsDeposit_WhenDepositIsNegative()
    {
        var mockRepo = new Mock<IOnlineWalletRepository>();
        mockRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = 0 });

        var service = new OnlineWalletService(mockRepo.Object);
        var deposit = new Deposit { Amount = -100 };

        var afterDeposit = await service.DepositFundsAsync(deposit);
        // Note: This unit test does not throw an exception for a negative deposit amount because
        // the validation for deposit amount is handled at the API layer through FluentValidation, not at the service layer.
        // This seems to be the intentional design, as the service layer relies on the API layer's validation to prevent negative deposits.
    }
}