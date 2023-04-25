using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace PumpkinMoon.Core.Reflection;

public static class ReflectionUtils
{
    public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private struct MethodKey
    {
        public Type Type;
        public string MethodName;
    }

    private static readonly Dictionary<MethodKey, MethodBase> CachedMethods =
        new Dictionary<MethodKey, MethodBase>();

    public static MethodBase GetMethod(Type type, string methodName, params Type[] parameters)
    {
        MethodKey key = new MethodKey { Type = type, MethodName = methodName };

        if (CachedMethods.TryGetValue(key, out MethodBase result))
        {
            return result;
        }

        return CachedMethods[key] =
            type.GetMethod(methodName, Flags, null, CallingConventions.Any, parameters, null);
    }

    public static MethodBase GetMethod(Type type, string methodName)
    {
        MethodKey key = new MethodKey { Type = type, MethodName = methodName };

        if (CachedMethods.TryGetValue(key, out MethodBase result))
        {
            return result;
        }

        return CachedMethods[key] = type.GetMethod(methodName, Flags);
    }

    public static MethodBase GetMethod<T>(Expression<T> expression) where T : Delegate
    {
        switch (expression.Body)
        {
            case MethodCallExpression methodCallExpression:
                return methodCallExpression.Method;
            case NewExpression newExpression:
                return newExpression.Constructor;
            case MemberExpression memberExpression:
            {
                Type delegateType = typeof(T).GetGenericTypeDefinition();

                bool isFunc = delegateType == typeof(Func<>) ||
                              delegateType == typeof(Func<,>) ||
                              delegateType == typeof(Func<,,>) ||
                              delegateType == typeof(Func<,,,>);

                MemberInfo member = memberExpression.Member;

                if (member is PropertyInfo propertyInfo)
                {
                    return isFunc ? propertyInfo.GetMethod : propertyInfo.SetMethod;
                }

                return null;
            }
            case var _:
                return null;
        }
    }

    public static MethodBase GetMethod<T>(Expression<Action<T>> expression)
    {
        return GetMethod<Action<T>>(expression);
    }

    public static MethodBase GetMethod<T>(Expression<Func<T>> expression)
    {
        return GetMethod<Func<T>>(expression);
    }

    public static MethodBase GetMethod<T0, T1>(Expression<Action<T0, T1>> expression)
    {
        return GetMethod<Action<T0, T1>>(expression);
    }

    public static MethodBase GetMethod<T0, T1>(Expression<Func<T0, T1>> expression)
    {
        return GetMethod<Func<T0, T1>>(expression);
    }

    public static MethodBase GetMethod<T0, T1, T2>(Expression<Action<T0, T1, T2>> expression)
    {
        return GetMethod<Action<T0, T1, T2>>(expression);
    }

    public static MethodBase GetMethod<T0, T1, T2>(Expression<Func<T0, T1, T2>> expression)
    {
        return GetMethod<Func<T0, T1, T2>>(expression);
    }

    public static MethodBase GetMethod<T0, T1, T2, T3>(Expression<Action<T0, T1, T2, T3>> expression)
    {
        return GetMethod<Action<T0, T1, T2, T3>>(expression);
    }

    public static MethodBase GetMethod<T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3>> expression)
    {
        return GetMethod<Func<T0, T1, T2, T3>>(expression);
    }
}