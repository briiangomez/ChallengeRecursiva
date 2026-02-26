using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using HoroscopeChallenge.Api.Domain.Entities;
using HoroscopeChallenge.Api.DTOs;
using HoroscopeChallenge.Api.Repositories;
using HoroscopeChallenge.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace HoroscopeChallenge.Tests.Unit;

public class HoroscopeServiceTests
{

    private static readonly User SampleUser = new()
    {
        Id        = 1,
        Username  = "testuser",
        Email     = "test@example.com",
        BirthDate = new DateOnly(1992, 8, 9), // Leo
        CreatedAt = DateTime.UtcNow
    };

    private const string SampleJson = """{"horoscope":"Será un gran día para Leo.","sign":"leo"}""";


    private static HoroscopeService BuildService(
        Mock<IUserRepository>? userRepo = null,
        Mock<IHoroscopeCacheRepository>? cacheRepo = null,
        Mock<IHoroscopeQueryHistoryRepository>? historyRepo = null,
        HttpMessageHandler? httpHandler = null,
        IMemoryCache? memoryCache = null)
    {
        userRepo    ??= BuildDefaultUserRepo();
        cacheRepo   ??= new Mock<IHoroscopeCacheRepository>();
        historyRepo ??= new Mock<IHoroscopeQueryHistoryRepository>();

        var handler = httpHandler ?? BuildHttpHandler(SampleJson);
        var client  = new HttpClient(handler) { BaseAddress = new Uri("https://newastro.vercel.app") };

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient("HoroscopeApi")).Returns(client);

        memoryCache ??= new MemoryCache(new MemoryCacheOptions());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HoroscopeApi:BaseUrl"]    = "https://newastro.vercel.app",
                ["HoroscopeApi:DefaultLang"] = "es"
            })
            .Build();

        return new HoroscopeService(
            userRepo.Object,
            cacheRepo.Object,
            historyRepo.Object,
            httpFactory.Object,
            memoryCache,
            config
        );
    }

    private static Mock<IUserRepository> BuildDefaultUserRepo()
    {
        var mock = new Mock<IUserRepository>();
        mock.Setup(r => r.GetByIdAsync(SampleUser.Id)).ReturnsAsync(SampleUser);
        return mock;
    }

    private static HttpMessageHandler BuildHttpHandler(string responseBody)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        return handlerMock.Object;
    }

    [Fact]
    public async Task GetTodayAsync_WhenMemoryCacheHit_ShouldNotCallDbNorHttp()
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var cacheKey  = $"horoscope:leo:{today:yyyy-MM-dd}:es";
        var cache     = new MemoryCache(new MemoryCacheOptions());
        cache.Set(cacheKey, SampleJson);

        var cacheRepo   = new Mock<IHoroscopeCacheRepository>();
        var historyRepo = new Mock<IHoroscopeQueryHistoryRepository>();
        var httpCalled  = false;
        var handler     = new CallTrackingHandler(() => httpCalled = true, SampleJson);

        var service = BuildService(
            cacheRepo:   cacheRepo,
            historyRepo: historyRepo,
            httpHandler: handler,
            memoryCache: cache
        );

        // Act
        var result = await service.GetTodayAsync(SampleUser.Id, "es");

        // Assert
        result.Should().NotBeNull();
        result.Sign.Should().Be("leo");
        cacheRepo.Verify(r => r.GetAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string>()), Times.Never);
        httpCalled.Should().BeFalse("no debería llamar a la API si MemoryCache tiene el dato");
    }

    [Fact]
    public async Task GetTodayAsync_WhenDbCacheHit_ShouldNotCallHttpButPopulateMemoryCache()
    {
        var today   = DateOnly.FromDateTime(DateTime.UtcNow);
        var dbEntry = new HoroscopeCache { Sign = "leo", Date = today, Lang = "es", ResponseJson = SampleJson };

        var cacheRepo = new Mock<IHoroscopeCacheRepository>();
        cacheRepo.Setup(r => r.GetAsync("leo", today, "es")).ReturnsAsync(dbEntry);

        var httpCalled = false;
        var handler    = new CallTrackingHandler(() => httpCalled = true, SampleJson);
        var memCache   = new MemoryCache(new MemoryCacheOptions());

        var service = BuildService(cacheRepo: cacheRepo, httpHandler: handler, memoryCache: memCache);

        // Act
        var result = await service.GetTodayAsync(SampleUser.Id, "es");

        // Assert
        result.Sign.Should().Be("leo");
        httpCalled.Should().BeFalse("no debería llamar a la API si la DB tiene el cache");

        var cacheKey = $"horoscope:leo:{today:yyyy-MM-dd}:es";
        memCache.TryGetValue(cacheKey, out string? cached).Should().BeTrue();
        cached.Should().Be(SampleJson);
    }

    [Fact]
    public async Task GetTodayAsync_WhenCacheMiss_ShouldCallHttpAndPersistInDbAndMemory()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var cacheRepo = new Mock<IHoroscopeCacheRepository>();
        cacheRepo.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string>()))
                 .ReturnsAsync((HoroscopeCache?)null);

        var httpCalled = false;
        var handler    = new CallTrackingHandler(() => httpCalled = true, SampleJson);
        var memCache   = new MemoryCache(new MemoryCacheOptions());

        var service = BuildService(cacheRepo: cacheRepo, httpHandler: handler, memoryCache: memCache);

        // Act
        var result = await service.GetTodayAsync(SampleUser.Id, "es");

        // Assert
        httpCalled.Should().BeTrue("debe llamar a la API cuando no hay cache");
        result.Sign.Should().Be("leo");
        result.Horoscope.Should().Contain("Leo");

        cacheRepo.Verify(r => r.CreateAsync(It.Is<HoroscopeCache>(c =>
            c.Sign == "leo" && c.Date == today && c.Lang == "es")), Times.Once);

        var cacheKey = $"horoscope:leo:{today:yyyy-MM-dd}:es";
        memCache.TryGetValue(cacheKey, out string? _).Should().BeTrue();
    }

    [Fact]
    public async Task GetTodayAsync_ShouldAlwaysPersistQueryHistory()
    {
        var historyRepo = new Mock<IHoroscopeQueryHistoryRepository>();
        var cacheRepo   = new Mock<IHoroscopeCacheRepository>();
        cacheRepo.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string>()))
                 .ReturnsAsync((HoroscopeCache?)null);

        var service = BuildService(cacheRepo: cacheRepo, historyRepo: historyRepo);

        await service.GetTodayAsync(SampleUser.Id, "es");

        historyRepo.Verify(r => r.CreateAsync(It.Is<HoroscopeQueryHistory>(h =>
            h.UserId == SampleUser.Id && h.Sign == "leo")), Times.Once);
    }

    private class CallTrackingHandler : HttpMessageHandler
    {
        private readonly Action _onCall;
        private readonly string _responseBody;

        public CallTrackingHandler(Action onCall, string responseBody)
        {
            _onCall       = onCall;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _onCall();
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
