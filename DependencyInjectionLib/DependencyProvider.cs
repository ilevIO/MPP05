using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DependencyInjectionLib
{
    public class DependencyProvider
    {
        DependenciesConfiguration dependencies;
        ConcurrentDictionary<int, Stack<Type>> inProcess = new ConcurrentDictionary<int, Stack<Type>>();
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
        bool TypeIsEnumerable(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }
        IList<object> GetConstructedInstances(Type tImplementation)
        {
            IList<object> result = new List<object>();
            var constructors = tImplementation.GetConstructors();
            foreach(ConstructorInfo constructor in constructors)
            {
                IList<object> arguments = new List<object>();
                var parameters = constructor.GetParameters();
                foreach (ParameterInfo parameter in parameters)
                {
                    arguments.Add(this.Resolve(parameter.ParameterType));
                }
                object instance = null;
                try
                {
                    //Does not work without ".ToArray()"
                    instance = Activator.CreateInstance(tImplementation, arguments.ToArray());
                } catch
                {
                    //leave null
                }
                result.Add(instance);
            }
            return result;
        }
        public object Resolve(Type tDependency)
        {
            //get implementations for tDependency
            //for each implementation find constructors with existing implementations for dependencies
            //find constructors with registered
            bool isEnumerable = TypeIsEnumerable(tDependency);
            object result = null;
            int currThreadId = Thread.CurrentThread.ManagedThreadId;
            bool resolved = false;
            if (!IsBeingResolved(tDependency, currThreadId))
            {
                if (!inProcess.TryGetValue(currThreadId, out Stack<Type> currentStack))
                {
                    currentStack = new Stack<Type>();
                }
                currentStack.Push(tDependency);
                inProcess[currThreadId] = currentStack;
                IList<Implementation> implementations = dependencies.GetImplementationsFor(tDependency);
                if (implementations != null)
                {
                    IList<object> instances = new List<object>();
                    for (int i = 0; i < implementations.Count; i++)
                    {
                        instances.Add(implementations[i].GetInstance(this));
                    }
                    if (!isEnumerable)
                    {
                        resolved = true;
                        result = instances.First();
                        //return instances.First();
                    }
                    else
                    {
                        resolved = true;
                        result = instances;
                    }
                    //return instances;
                } else
                {
                    var instances = this.GetConstructedInstances(tDependency);
                    if (isEnumerable)
                    {
                        resolved = true;
                        result = instances;
                    } else
                    {
                        resolved = true;
                        result = instances.First();
                    }
                }
                currentStack.Pop();
            }
            if (!resolved)
            {
                //default value:
                //what will it return for an interface?
                object instance = null;
                try
                {
                    instance = Activator.CreateInstance(tDependency);
                }
                catch
                {
                    //could not create
                }
                return instance;
            }
            return result;
            //return null;
        }
        public DependencyProvider(DependenciesConfiguration dependencies)
        {
            this.dependencies = dependencies;
        }
    }
}
