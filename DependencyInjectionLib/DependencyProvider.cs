using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionLib
{
    public class DependencyProvider
    {
        public TDependency Resolve<TDependency>()
        {
            return (TDependency)(Resolve(typeof(TDependency)));
        }
        object Resolve(Type tDependency)
        {
            return null;
        }
        public DependencyProvider(DependenciesConfiguration dependencies)
        {

        }
    }
}
