using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionLib
{
    public class DependenciesConfiguration
    {
        public void Register<TDependency, TImplementation>()
        {
            this.Register(typeof(TDependency), typeof(TImplementation));
        }
        void Register(Type tDependency, Type tImplementation)
        {

        }
    }
}
