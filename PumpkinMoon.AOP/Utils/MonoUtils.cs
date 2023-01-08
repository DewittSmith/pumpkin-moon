using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PumpkinMoon.AOP.Utils
{
    public static class MonoUtils
    {
        public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                          BindingFlags.NonPublic;

        public static T CreateDelegate<T>(MethodDefinition methodDefinition) where T : Delegate
        {
            MethodInfo methodInfo = GetMethod(methodDefinition);

            if (methodInfo.IsStatic)
            {
                return (T)methodInfo.CreateDelegate(typeof(T));
            }

            object instance = Activator.CreateInstance(methodInfo.DeclaringType!);
            return (T)methodInfo.CreateDelegate(typeof(T), instance);
        }

        public static Type GetType(TypeDefinition typeReference)
        {
            string typeName = typeReference.FullName;
            string assemblyName = typeReference.Module.Assembly.FullName;
            string assemblyQualifiedName = $"{typeName}, {assemblyName}";
            return Type.GetType(assemblyQualifiedName);
        }

        public static MethodInfo GetMethod(MethodDefinition methodDefinition)
        {
            Type type = GetType(methodDefinition.DeclaringType);
            return type.GetMethod(methodDefinition.Name, Flags);
        }

        public static ushort AddVariable(MethodDefinition methodDefinition, TypeReference varType)
        {
            methodDefinition.Body.Variables.Add(new VariableDefinition(varType));
            return (ushort)(methodDefinition.Body.Variables.Count - 1);
        }

        public static ushort AddVariable(MethodDefinition methodDefinition, Type varType)
        {
            ModuleDefinition module = methodDefinition.Module;
            return AddVariable(methodDefinition, module.ImportReference(varType));
        }
    }
}