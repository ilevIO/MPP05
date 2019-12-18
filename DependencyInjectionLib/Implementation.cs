using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjectionLib
{
    public class Implementation
    {
        public Type type;
        object instance;
        public DependencyTTL dependencyTTL;
        public object GetInstance(DependencyProvider provider)
        {
            if (this.dependencyTTL == DependencyTTL.SINGLETON)
            {
                return instance;
            }
            return provider.Resolve(type);
        }
        public Implementation(Type type, DependencyTTL dependencyTTL = DependencyTTL.IPD, DependencyProvider provider = null)
        {
            this.type = type;
            this.dependencyTTL = dependencyTTL;
            if (dependencyTTL == DependencyTTL.SINGLETON)
            {
                if (provider == null)
                {
                    //throw error
                } else {
                    this.instance = provider.Resolve(type);
                }
            }
        }
    }
}
