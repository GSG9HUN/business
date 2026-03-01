using DC_bot.Service;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.UnitTests.ServiceTest;

public class ServiceLocatorTests
{
    [Fact]
    public void SetServiceProvider_ShouldSetInstanceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        ServiceLocator.SetServiceProvider(serviceProvider);

        // Assert
        var resolvedService = ServiceLocator.GetService<ITestService>();
        Assert.NotNull(resolvedService);
        Assert.IsType<TestService>(resolvedService);
    }

    [Fact]
    public void GetService_ShouldReturnRegisteredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        // Act
        var resolvedService = ServiceLocator.GetService<ITestService>();

        // Assert
        Assert.NotNull(resolvedService);
        Assert.IsType<TestService>(resolvedService);
    }

    [Fact]
    public void GetServices_ShouldReturnAllRegisteredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService, AnotherTestService>();
        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        // Act
        var resolvedServices = ServiceLocator.GetServices<ITestService>();

        // Assert
        Assert.NotNull(resolvedServices);
        var testServices = resolvedServices as ITestService[] ?? resolvedServices.ToArray();
        Assert.Equal(2, testServices.Count());
        Assert.Contains(testServices, s => s is TestService);
        Assert.Contains(testServices, s => s is AnotherTestService);
    }

    [Fact]
    public void GetRequiredService_ThrowsException_WhenNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ServiceLocator.GetService<ITestService>());
    }
}

public interface ITestService
{
}

public class TestService : ITestService
{
}

public class AnotherTestService : ITestService
{
}