using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyInjectionLib;
using System.Collections.Generic;

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
    class ServiceImpl2: IService
    {
        public ServiceImpl2()
        {

        }
    }
    interface IRepository { }
    class RepositoryImpl: IRepository
    {
        public RepositoryImpl() { } // может иметь свои зависимости, опустим для простоты
    }
    /// 
    /// ///////////////////////////////////////////
    ///
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
        [TestMethod] 
        public void NumerousImplsTest()
        {
            IList<Type> implTypes = new List<Type>();
            implTypes.Add(typeof(ServiceImpl));
            implTypes.Add(typeof(ServiceImpl2));

            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImpl>();
            dependencies.Register<IService, ServiceImpl2>();

            var provider = new DependencyProvider(dependencies);

            // должен быть создан ServiceImpl (реализация IService), в конструктор которому передана
            // RepositoryImpl (реализация IRepository)
            var services =  provider.Resolve<IEnumerable<IService>>();//*/(List<IService>)provider.Resolve(typeof(List<IService>));// <List<IService>>();
            Assert.IsNotNull(services);
            //Assert.IsTrue(typeof(services[0].))
            foreach (var implType in implTypes)
            {
                bool exists = false;
                foreach (var service in services)
                {
                    if (service.GetType() == implType)
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsTrue(exists);
            }
        }
        [TestMethod] 
        public void SingletonTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImpl>(DependencyTTL.SINGLETON);

            var provider = new DependencyProvider(dependencies);
            var inst1 = provider.Resolve<IService>();
            var inst2 = provider.Resolve<IService>();
            Assert.AreSame(inst1, inst2);
        }
        interface IGeneric<T>
        {
        }
        class GenericClass<T>: IGeneric<T>
        {

        }
        class GenericClass2<T> : IGeneric<T>
        {

        }
        [TestMethod]
        public void GenericDependencyTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IGeneric<Service3>, GenericClass<Service3>>();

            var provider = new DependencyProvider(dependencies);
            var inst1 = provider.Resolve<IGeneric<Service3>>();

            Assert.AreEqual(typeof(GenericClass<Service3>), inst1.GetType());
        }
        [TestMethod]
        public void OpenGenericDependencyTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register(typeof(IGeneric<>), typeof(GenericClass<>));

            var provider = new DependencyProvider(dependencies);
            var inst1 = provider.Resolve<IGeneric<Service3>>();

            Assert.AreEqual(typeof(GenericClass<Service3>), inst1.GetType());
        }
        [TestMethod]
        public void MultipleGenericDependencyTest()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register(typeof(IGeneric<>), typeof(GenericClass<>));
            dependencies.Register(typeof(IGeneric<>), typeof(GenericClass2<>));
            dependencies.Register<IRepository, RepositoryImpl>();
            var provider = new DependencyProvider(dependencies);
            var inst1 = provider.Resolve<IGeneric<Service3>>();

            Assert.AreEqual(typeof(GenericClass<Service3>), inst1.GetType());
        }
    }
}
