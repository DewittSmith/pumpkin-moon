using Mono.Cecil;

namespace PumpkinMoon.AOP.Aspects
{
    public class WeaveArgs
    {
        public AssemblyDefinition Assembly { get; }
        public ModuleDefinition Module => Assembly.MainModule;
        public TypeDefinition Type { get; }
        public MethodDefinition EntryPoint { get; }

        public MethodDefinition CurrentMethod { get; }

        internal WeaveArgs(AssemblyDefinition assembly, TypeDefinition type, MethodDefinition entryPoint,
            MethodDefinition currentMethod)
        {
            Assembly = assembly;
            Type = type;
            EntryPoint = entryPoint;
            CurrentMethod = currentMethod;
        }
    }
}