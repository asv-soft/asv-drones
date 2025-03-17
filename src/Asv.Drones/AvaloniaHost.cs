using System;
using System.Composition.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;



public interface IContainerHost
{
    
}

public class MefContainerBuilder
{
    private CompositionHost _container;
    private readonly IServiceCollection _services;

    public MefContainerBuilder(IServiceCollection services)
    {
        _services = services;
        var configuration = new ContainerConfiguration();
        _container = configuration.CreateContainer(); // Инициализация пустого контейнера MEF
    }

    // Метод для добавления сборок или частей в контейнер MEF
    public MefContainerBuilder WithAssemblies(params System.Reflection.Assembly[] assemblies)
    {
        var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
        _container.Dispose(); // Удаляем старый контейнер
        _container = configuration.CreateContainer();
        return this;
    }

    public CompositionHost Container => _container;
    public IServiceCollection Services => _services;
}

public class MefServiceProviderFactory : IServiceProviderFactory<MefContainerBuilder>
{
    private readonly Action<MefContainerBuilder> _configureContainer;

    public MefServiceProviderFactory(Action<MefContainerBuilder> configureContainer = null)
    {
        _configureContainer = configureContainer ?? (builder => { });
    }

    // Создание объекта MefContainerBuilder для конфигурации
    public MefContainerBuilder CreateBuilder(IServiceCollection services)
    {
        var builder = new MefContainerBuilder(services);
        _configureContainer(builder); // Применяем пользовательскую конфигурацию
        return builder;
    }

    // Построение IServiceProvider на основе MEF
    public IServiceProvider CreateServiceProvider(MefContainerBuilder containerBuilder)
    {
        if (containerBuilder == null)
            throw new ArgumentNullException(nameof(containerBuilder));

        // Создаем обертку IServiceProvider, которая использует MEF
        return new MefServiceProvider(containerBuilder.Container, containerBuilder.Services);
    }
}

public class MefServiceProvider : IServiceProvider, IDisposable
{
    private readonly CompositionHost _container;
    private readonly IServiceProvider _fallbackProvider;

    public MefServiceProvider(CompositionHost container, IServiceCollection services)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));

        // Создаем стандартный провайдер для сервисов, зарегистрированных через IServiceCollection
        _fallbackProvider = services.BuildServiceProvider();
    }

    public object GetService(Type serviceType)
    {
        // Сначала пытаемся получить сервис из MEF
        try
        {
            // Export<T> в MEF возвращает экземпляр, если он есть
            var export = _container.GetExport(serviceType);
            if (export != null)
                return export;
        }
        catch (Exception)
        {
            // Если MEF не нашел сервис, переходим к fallback
        }

        // Если MEF не предоставил сервис, используем стандартный DI
        return _fallbackProvider.GetService(serviceType);
    }

    public void Dispose()
    {
        _container.Dispose();
        if (_fallbackProvider is IDisposable disposable)
            disposable.Dispose();
    }
}