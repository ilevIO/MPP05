using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DependencyInjectionLib
{
    public class DependencyProvider
    {
        DependenciesConfiguration dependencies;
        ConcurrentDictionary<int, Stack<Type>> inProcess;
        bool IsBeingResolved(Type type, int threadId)
        {
            Stack<Type> currentStack;
            if (inProcess.TryGetValue(threadId, out currentStack))
            {
                if (currentStack.Contains(type))
                {
                    return true;
                }
            }
            return false;
        }
        public TDependency Resolve<TDependency>()
        {
            return (TDependency)(Resolve(typeof(TDependency)));
        }
        public object Resolve(Type tDependency)
        {
            //get implementations for tDependency
            //for each implementation find constructors with existing implementations for dependencies
            //find constructors with registered
            int currThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!IsBeingResolved(tDependency, currThreadId))
            {
                if (!inProcess.TryGetValue(currThreadId, out Stack<Type> currentStack))
                {
                    currentStack = new Stack<Type>();
                }
                currentStack.Push(tDependency);

                IList<Implementation> implementations = dependencies.GetImplementationsFor(tDependency);
                if (implementations != null)
                {
                    IList<object> instances = new List<object>();
                    for (int i = 0; i < implementations.Count; i++)
                    {
                        instances.Add(implementations[i].GetInstance(this));
                    }
                    return implementations;
                }
                currentStack.Pop();
            }
            //default value:
            //what will it return for an interface?
            return Activator.CreateInstance(tDependency);
            //return null;
        }
        public DependencyProvider(DependenciesConfiguration dependencies)
        {
            this.dependencies = dependencies;
        }
    }
}
