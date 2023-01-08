using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PumpkinMoon.AOP.Utils;
using PumpkinMoon.Core.Reflection;

namespace PumpkinMoon.AOP.Aspects.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
    public class MethodDecoratorAspect : WeaveAspectBase
    {
        internal override void WeaveAspect(WeaveArgs args)
        {
            ModuleDefinition module = args.Module;

            var instructions = args.CurrentMethod.Body.Instructions;

            ushort loc0 = MonoUtils.AddVariable(args.CurrentMethod, typeof(MethodInfo));
            ushort loc1 = MonoUtils.AddVariable(args.CurrentMethod, typeof(MethodDecoratorAspect));
            ushort loc2 = MonoUtils.AddVariable(args.CurrentMethod, typeof(MethodExecutionArgs));

            ILProcessor ilProcessor = args.CurrentMethod.Body.GetILProcessor();

            Instruction firstInstruction = instructions.First();
            Instruction lastInstruction = instructions.Last();

            MethodInfo getTypeMethod = (MethodInfo)ReflectionUtils.GetMethod((object obj) => obj.GetType());

            void CreateMethodArgs()
            {
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldarg_0));

                ilProcessor.InsertBefore(firstInstruction,
                    ilProcessor.Create(OpCodes.Call, module.ImportReference(getTypeMethod)));

                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, args.CurrentMethod.Name));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4_S, (sbyte)20));

                MethodInfo getMethodMethod =
                    (MethodInfo)ReflectionUtils.GetMethod((string name, BindingFlags flags, Type type) =>
                        type.GetMethod(name, flags));

                ilProcessor.InsertBefore(firstInstruction,
                    ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(getMethodMethod)));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, loc0));

                MethodInfo getAttributeMethod = (MethodInfo)ReflectionUtils.GetMethod((MethodInfo methodInfo) =>
                    methodInfo.GetCustomAttribute<MethodDecoratorAspect>());

                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, loc0));

                ilProcessor.InsertBefore(firstInstruction,
                    ilProcessor.Create(OpCodes.Call, module.ImportReference(getAttributeMethod)));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, loc1));

                ConstructorInfo createArgsMethod = (ConstructorInfo)ReflectionUtils.GetMethod(
                    (MethodBase methodBase, object instance) => new MethodExecutionArgs(methodBase, instance));

                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, loc0));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldarg_0));

                ilProcessor.InsertBefore(firstInstruction,
                    ilProcessor.Create(OpCodes.Newobj, module.ImportReference(createArgsMethod)));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, loc2));
            }

            CreateMethodArgs();

            Instruction loadLoc1 = ilProcessor.Create(OpCodes.Ldloc, loc1);
            Instruction loadLoc2 = ilProcessor.Create(OpCodes.Ldloc, loc2);

            ilProcessor.InsertBefore(firstInstruction, loadLoc1);
            ilProcessor.InsertBefore(firstInstruction, loadLoc2);

            // attribute.OnEntered(args)
            MethodInfo enterMethod = (MethodInfo)ReflectionUtils.GetMethod(
                (MethodDecoratorAspect aspect, MethodExecutionArgs args) =>
                    aspect.OnEntered(args));

            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(enterMethod)));

            ilProcessor.InsertBefore(lastInstruction, loadLoc1);
            ilProcessor.InsertBefore(lastInstruction, loadLoc2);

            // attribute.OnSuccess(args)
            MethodInfo successMethod =
                (MethodInfo)ReflectionUtils.GetMethod((MethodDecoratorAspect aspect, MethodExecutionArgs args) =>
                    aspect.OnSuccess(args));
            ilProcessor.InsertBefore(lastInstruction,
                ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(successMethod)));

            Instruction leave = ilProcessor.Create(OpCodes.Leave, lastInstruction);
            Instruction pop = ilProcessor.Create(OpCodes.Pop);
            Instruction rethrow = ilProcessor.Create(OpCodes.Rethrow);
            Instruction nop = ilProcessor.Create(OpCodes.Nop);

            ilProcessor.InsertBefore(lastInstruction, leave);
            ilProcessor.InsertAfter(leave, pop);
            ilProcessor.InsertAfter(pop, rethrow);
            ilProcessor.InsertAfter(rethrow, nop);

            // attribute.OnException(args)
            MethodInfo exceptionMethod =
                (MethodInfo)ReflectionUtils.GetMethod((MethodDecoratorAspect aspect, MethodExecutionArgs args) =>
                    aspect.OnException(args));

            ilProcessor.InsertAfter(pop,
                ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(exceptionMethod)));

            ilProcessor.InsertAfter(pop, loadLoc2);
            ilProcessor.InsertAfter(pop, loadLoc1);

            ExceptionHandler finallyInstruction = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = firstInstruction,
                TryEnd = nop,
                HandlerStart = nop,
                HandlerEnd = lastInstruction
            };

            ExceptionHandler catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = firstInstruction,
                TryEnd = pop,
                HandlerStart = pop,
                HandlerEnd = nop,
                CatchType = module.ImportReference(typeof(object))
            };

            args.CurrentMethod.Body.ExceptionHandlers.Add(catchHandler);
            args.CurrentMethod.Body.ExceptionHandlers.Add(finallyInstruction);

            ilProcessor.InsertBefore(lastInstruction, loadLoc1);
            ilProcessor.InsertBefore(lastInstruction, loadLoc2);

            // attribute.OnExited(args)
            MethodInfo exitMethod =
                (MethodInfo)ReflectionUtils.GetMethod((MethodDecoratorAspect aspect, MethodExecutionArgs args) =>
                    aspect.OnExited(args));

            ilProcessor.InsertBefore(lastInstruction,
                ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(exitMethod)));

            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Endfinally));
        }

        public virtual void RuntimeInitialize(MethodBase method)
        {
        }

        public virtual void OnEntered(MethodExecutionArgs args)
        {
        }

        public virtual void OnSuccess(MethodExecutionArgs args)
        {
        }

        public virtual void OnExited(MethodExecutionArgs args)
        {
        }

        public virtual void OnException(MethodExecutionArgs args)
        {
        }
    }
}