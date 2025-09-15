using NUnit.Framework;
using DinoServer.Users;
using DinoServer.Services;
using DinoServer.Controllers;
using DinoServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TUser = DinoServer.Users.User; // alias, чтобы избежать конфликта с Telegram.Bot.Types.User

namespace DinoServer.Tests
{
  [TestFixture]
  public class FullCoverageTests
  {
    private UserContext _context = null!;
    private IDbContextFactory<UserContext> _factory = null!;
    private AddUserService _addService = null!;
    private GetUsersService _getService = null!;
    private UserController _controller = null!;

    [SetUp]
    public void Setup()
    {
      var options = new DbContextOptionsBuilder<UserContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new UserContext(options);

      var mockFactory = new Mock<IDbContextFactory<UserContext>>();
      mockFactory.Setup(f => f.CreateDbContext()).Returns(_context);
      _factory = mockFactory.Object;

      typeof(TelegramService).GetField("_contextFactory", BindingFlags.NonPublic | BindingFlags.Static)
          ?.SetValue(null, _factory);

      _addService = new AddUserService(_factory);
      _getService = new GetUsersService(_factory);
      _controller = new UserController(_getService, _addService);

      // Очистка _chatIds TelegramService
      var chatIdsField = typeof(TelegramService).GetField("_chatIds", BindingFlags.NonPublic | BindingFlags.Static);
      var chatIdsInstance = (ConcurrentDictionary<long, bool>)chatIdsField!.GetValue(null)!;
      chatIdsInstance.Clear();

      // Устанавливаем _bot = null
      typeof(TelegramService).GetField("_bot", BindingFlags.NonPublic | BindingFlags.Static)
          ?.SetValue(null, null);
    }

    [TearDown]
    public void TearDown()
    {
      _context.Dispose();
    }

    [Test]
    public async Task AddUserService_AddUser_ShouldAddUser()
    {
      var user = new TUser { Name = "Alice", Score = 100 };
      var result = await _addService.AddUserAsync(user, 1);
      Assert.IsNotNull(result);
      Assert.AreEqual("Alice", result.Name);
    }

    [Test]
    public void AddUserService_AddNullUser_ShouldThrow()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _addService.AddUserAsync(null!, 1));
    }

    [Test]
    public async Task GetUsersService_ShouldReturnAllUsers()
    {
      await _context.AddUserAsync(new TUser("Bob", 50));
      await _context.AddUserAsync(new TUser("Carol", 75));

      var users = await _getService.GetUsersAsync();
      Assert.AreEqual(2, users.Count());
    }

    [Test]
    public async Task UserController_Get_ShouldReturnOkWithUsers()
    {
      await _context.AddUserAsync(new TUser("Dave", 20));
      var result = await _controller.Get();
      var okResult = result.Result as OkObjectResult;
      Assert.IsNotNull(okResult);
    }

    [Test]
    public async Task UserController_AddBook_ShouldAddUserAndReturnOk()
    {
      var user = new TUser { Name = "Eve", Score = 99 };
      var result = await _controller.AddBook(user, 42);
      var okResult = result as OkObjectResult;
      Assert.IsNotNull(okResult);
    }

    [Test]
    public async Task UserController_AddBook_NullUser_ShouldReturnBadRequest()
    {
        var result = await _controller.AddBook(null!, 1);
      Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    // --- TelegramService tests ---
    [Test]
    public async Task TelegramService_SendMessage_NoBot_ShouldNotThrow()
    {
      await TelegramService.SendMessage("Test message");
    }

    [Test]
    public async Task TelegramService_BuildLeaderboard_ShouldReturnCorrectString()
    {
      await _context.AddUserAsync(new TUser("User1", 10));
      await _context.AddUserAsync(new TUser("User2", 20));

        var method = typeof(TelegramService).GetMethod("BuildLeaderboardAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
        var task = (Task<string>)method.Invoke(null, null)!;
        var result = await task;

      Assert.IsTrue(result.Contains("User2"));
      Assert.IsTrue(result.Contains("User1"));
    }
  }
}
