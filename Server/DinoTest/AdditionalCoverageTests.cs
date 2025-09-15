using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DinoServer;
using DinoServer.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TUser = DinoServer.Users.User;

namespace DinoServer.Tests
{
    [TestFixture]
    public class AdditionalCoverageTests
    {
        [Test]
        public void User_ToString_Equals_GetHashCode()
        {
            var u1 = new TUser("Ann", 5);
            var u2 = new TUser("Ann", 5);
            var u3 = new TUser("Bob", 7);

            Assert.AreEqual("Ann, 5", u1.ToString());
            Assert.IsTrue(u1.Equals(u2));
            Assert.IsFalse(u1.Equals(u3));
            Assert.AreEqual(u1.GetHashCode(), u2.GetHashCode());
        }

        [Test]
        public void UserContext_AddBook_ShouldAdd()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var ctx = new UserContext(options);
            ctx.AddBook(new TUser("Foo", 1));
            Assert.AreEqual(1, ctx.Users.Count());
        }

        [Test]
        public void TelegramService_Stop_ShouldCancel()
        {
            var ctsField = typeof(TelegramService).GetField("_cts", BindingFlags.NonPublic | BindingFlags.Static);
            var cts = new CancellationTokenSource();
            ctsField!.SetValue(null, cts);
            TelegramService.Stop();
            Assert.IsTrue(cts.IsCancellationRequested);
        }

        [Test]
        public async Task TelegramService_BuildLeaderboard_NoUsers_ReturnsMessage()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var ctx = new UserContext(options);
            var factoryMock = new Mock<IDbContextFactory<UserContext>>();
            factoryMock.Setup(f => f.CreateDbContext()).Returns(ctx);
            typeof(TelegramService).GetField("_contextFactory", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, factoryMock.Object);

            var method = typeof(TelegramService).GetMethod("BuildLeaderboardAsync", BindingFlags.NonPublic | BindingFlags.Static);
            var task = (Task<string>)method!.Invoke(null, null)!;
            var result = await task;
            Assert.AreEqual("Лидеры пока отсутствуют.", result.Trim());
        }

    }
}
