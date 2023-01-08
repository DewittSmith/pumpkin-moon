using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PumpkinMoon.AOP.Utils;
using PumpkinMoon.Core.Reflection;

namespace PumpkinMoon.AOP.Aspects.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class InitializeOnLoadAspect : WeaveAspectBase
    {
        internal override void WeaveAspect(WeaveArgs args)
        {
            ModuleDefinition module = args.Module;

            ILProcessor ilProcessor = args.EntryPoint.Body.GetILProcessor();
            Instruction firstInstruction = args.EntryPoint.Body.Instructions.First();

            MethodInfo getTypeMethodInfo =
                (MethodInfo)ReflectionUtils.GetMethod((RuntimeTypeHandle handle) => Type.GetTypeFromHandle(handle));

            MethodInfo getTypeHandleMethodInfo = (MethodInfo)ReflectionUtils.GetMethod((Type type) => type.TypeHandle);

            MethodInfo runClassConstructorMethodInfo = (MethodInfo)ReflectionUtils.GetMethod((RuntimeTypeHandle handle) =>
                RuntimeHelpers.RunClassConstructor(handle));

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldtoken, args.Type));
            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Call, module.ImportReference(getTypeMethodInfo)));
            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(getTypeHandleMethodInfo)));
            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Call, module.ImportReference(runClassConstructorMethodInfo)));
        }
    }
}