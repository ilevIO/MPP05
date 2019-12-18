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
        /*public object ConvertList(List<object> items, Type type, bool performConversion = false)
        {
            var containedType = type.GenericTypeArguments.First();
            var enumerableType = typeof(System.Linq.Enumerable);
            var castMethod = enumerableType.GetMethod(nameof(System.Linq.Enumerable.Cast)).MakeGenericMethod(containedType);
            var toListMethod = enumerableType.GetMethod(nameof(System.Linq.Enumerable.ToList)).MakeGenericMethod(containedType);

            IEnumerable<object> itemsToCast;

            if (performConversion)
            {
                itemsToCast = items.Select(item => Convert.ChangeType(item, containedType));
            }
            else
            {
                itemsToCast = items;
            }

            var castedItems = castMethod.Invoke(null, new[] { itemsToCast });

            return toListMethod.Invoke(null, new[] { castedItems });
        }*/
        public TDependency Resolve<TDependency>()
        {
            /*if (TypeIsEnumerable(typeof(TDependency))) 
            {
                var instances = (List<object>)Resolve(typeof(TDependency));
                var tDependency = typeof(TDependency).GetGenericArguments()[0];
                return (TDependency)ConvertList(instances, typeof(TDependency), true);
                //return instances.Select(item => Convert.ChangeType(item, containedType)).ToList().Cast<TDependency>();
                /*TDependency second = instances.Cast<SomethingElse>();
                return (TDependency)instances;
                foreach (var instance in instances)
                {
                    try {
                        var casted = Convert.ChangeType(instance, tDependency);
                        
                    } catch
                    {
                        //could not cast
                    }
                }
            } */
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
        private void ResolveIfContainsGenericParameter(ref Type type)
        {
            if (type.ContainsGenericParameters)
            {
                Type[] toResolve = type.GetGenericArguments();
                Type[] genericParameters = toResolve.Select(dep =>
                {
                    var impls = dependencies.GetImplementationsFor(dep.BaseType)?.ToArray();
                    return impls != null ? impls.First().type : dep.BaseType;
                })
                .ToArray();
                type = type.MakeGenericType(genericParameters);
            }
        }
        List<object> GetConstructedInstances(Type tImplementation)
        {
            ResolveIfContainsGenericParameter(ref tImplementation);
            List<object> result = new List<object>();
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
            if (isEnumerable)
            {
                tDependency = tDependency.GetGenericArguments()[0];
            }
            if (!IsBeingResolved(tDependency, currThreadId))
            {
                if (!inProcess.TryGetValue(currThreadId, out Stack<Type> currentStack))
                {
                    currentStack = new Stack<Type>();
                }
                currentStack.Push(tDependency);
                inProcess[currThreadId] = currentStack;
                IList<Implementation> implementations = dependencies.GetImplementationsFor(tDependency);
                if (implementations == null && tDependency.IsGenericType)
                {
                    implementations = dependencies.GetImplementationsFor(tDependency.GetGenericTypeDefinition());
                }
                if (implementations != null)
                {
                    //IList<object> instances = new List<object>();
                    var genericParams = tDependency.GetGenericArguments();
                    var instances = (object[])Activator.CreateInstance(tDependency.MakeArrayType(), new object[] { implementations.Count() });
                    for (int i = 0; i < implementations.Count; i++)
                    {
                        //var templ = new Implementation(implementations[i].type.GetGenericTypeDefinition().MakeGenericType(genericParams),
                        //implementations[i].dependencyTTL);
                        Implementation templ;
                        if (tDependency.IsGenericType)
                        {
                            templ = new Implementation(implementations[i].type.GetGenericTypeDefinition().MakeGenericType(genericParams),
                                implementations[i].dependencyTTL);
                        }
                        else
                        {
                            templ = implementations[i];
                        }
                        var inst = templ.GetInstance(this);
                        /*if (tDependency.IsGenericType) {
                            inst = inst.GetType().GetGenericTypeDefinition().MakeGenericType(genericParams);
                        }*/
                        instances[i] = inst;
                    }
                    if (!isEnumerable && instances.Length > 0)
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
                    object[] res;
                    if (!tDependency.ContainsGenericParameters)
                    {
                        res = (object[])Activator.CreateInstance(tDependency.MakeArrayType(), new object[] { instances.Count() });
                    } else
                    {
                        res = new object[instances.Count];
                        /*for (int i = 0; i < res.Length; i++)
                        {
                            res[i] = instances[i];
                        }*/
                    }

                    for (int i = 0; i < res.Length; i++)
                    {
                        res[i] = instances[i];
                    }
                    if (isEnumerable)
                    {
                        resolved = true;
                        result = res;
                    } else if (res.Length > 0)
                    {
                        resolved = true;
                        result = res[0];
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
