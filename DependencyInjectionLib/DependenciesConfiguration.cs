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
        ConcurrentDictionary<Type, IList<Implementation>> Implementations = new ConcurrentDictionary<Type, IList<Implementation>>();
        public IList<Implementation> GetImplementationsFor(Type tDependency)
        {
            this.Implementations.TryGetValue(tDependency, out IList<Implementation> implementations);
            return implementations;
        }
        bool RegistrationIsValid(Type tDependency, Type tImplementation)
        {
            if (tDependency.IsAssignableFrom(tImplementation) || (tDependency.IsGenericTypeDefinition && tImplementation.IsGenericTypeDefinition))
            {
                return true;
            }
            return false;
        }
        public void Register<TDependency, TImplementation>(DependencyTTL dependencyTTL = DependencyTTL.IPD)
        {
            this.Register(typeof(TDependency), typeof(TImplementation), dependencyTTL);
        }
        public void Register(Type tDependency, Type tImplementation, DependencyTTL dependencyTTL = DependencyTTL.IPD)
        {
            if (RegistrationIsValid(tDependency, tImplementation))
            {
                if (!Implementations.TryGetValue(tDependency, out IList<Implementation> implementations))
                {
                    implementations = new List<Implementation>();
                }
                if (implementations.Where(impl => impl.type == tImplementation && impl.dependencyTTL == dependencyTTL).Count() == 0)
                {
                    implementations.Add(new Implementation(tImplementation, dependencyTTL));
                    Implementations[tDependency] = implementations;
                }
            } else
            {
                throw new Exception("Is not valid");
            }
        }
    }
}
