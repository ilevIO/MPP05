using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionLib
{
    public enum DependencyTTL
    {
        //Instance Per Dependency
        IPD, 
        SINGLETON
    }
    public class DependenciesConfiguration
    {
        ConcurrentDictionary<Type, IList<Implementation>> Implementations;
        public IList<Implementation> GetImplementationsFor(Type tDependency)
        {
            this.Implementations.TryGetValue(tDependency, out IList<Implementation> implementations);
            return implementations;
        }
        public void Register<TDependency, TImplementation>(DependencyTTL dependencyTTL = DependencyTTL.IPD)
        {
            this.Register(typeof(TDependency), typeof(TImplementation), dependencyTTL);
        }
        void Register(Type tDependency, Type tImplementation, DependencyTTL dependencyTTL = DependencyTTL.IPD)
        {
            IList<Implementation> implementations;
            if (!Implementations.TryGetValue(tDependency, out implementations))
            {
                implementations = new List<Implementation>();
            }
            implementations.Add(new Implementation(tImplementation));
            Implementations[tDependency] = implementations;
        }
    }
}
