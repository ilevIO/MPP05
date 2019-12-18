using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyInjectionLib;

namespace DepentencyInjectionTests
{
    interface IService1
    {
        void SomeMethod();
    }
    class Service1: IService1
    {
        public void SomeMethod()
        {
            System.Console.WriteLine("SomeMethod");
        }
    }
    abstract class AbstractService2
    {

    }
    class Service2: AbstractService2
    {

    }
    class Service3
    {

    }
    //
    interface IService
    {
    }
    class ServiceImpl: IService
    {
        public ServiceImpl(IRepository repository) // ServiceImpl зависит от IRepository
        {
        }
    }

    interface IRepository { }
    class RepositoryImpl: IRepository
    {
        public RepositoryImpl() { } // может иметь свои зависимости, опустим для простоты
    }
    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void InterfaceTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService1, Service1>();
            var provider = new DependencyProvider(dependencies);
            var service1 = provider.Resolve<IService1>();
            Assert.IsNotNull(service1);
        }
        [TestMethod]
        public void AbstractTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<AbstractService2, Service2>();
            var provider = new DependencyProvider(dependencies);
            var service2 = provider.Resolve<AbstractService2>();
            Assert.IsNotNull(service2);
        }
        [TestMethod]
        public void AsSelfTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<Service3, Service3>();
            var provider = new DependencyProvider(dependencies);
            var service3 = provider.Resolve<Service3>();
            Assert.IsNotNull(service3);
        }
        [TestMethod]
        public void RecursiveTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImpl>();
            dependencies.Register<IRepository, RepositoryImpl>();

            var provider = new DependencyProvider(dependencies);

            // должен быть создан ServiceImpl (реализация IService), в конструктор которому передана
            // RepositoryImpl (реализация IRepository)
            var service1 = provider.Resolve<IService>();
            Assert.IsNotNull(service1);
        }
    }
}
