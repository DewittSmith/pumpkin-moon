using System;
using System.Reflection;

namespace PumpkinMoon.AOP.Aspects.Attributes
{
    public class MethodExecutionArgs
    {
        public MethodBase Method { get; }
        public object Instance { get; }

        public FlowBehavior FlowBehavior;

        public Exception Exception;

        public MethodExecutionArgs(MethodBase method, object instance)
        {
            Method = method;
            Instance = instance;
            FlowBehavior = FlowBehavior.Continue;
        }
    }
}