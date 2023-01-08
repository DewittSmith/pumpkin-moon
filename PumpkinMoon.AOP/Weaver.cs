using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PumpkinMoon.AOP.Aspects;
using PumpkinMoon.AOP.Utils;

namespace PumpkinMoon.AOP
{
    public static class Weaver
    {
        private static ILProcessor ilProcessor;

        private static readonly List<ModuleDefinition> Modules = new List<ModuleDefinition>();

        private static readonly Dictionary<TypeReference, WeaveDelegate> Aspects =
            new Dictionary<TypeReference, WeaveDelegate>();

        private static Dictionary<TypeReference, WeaveDelegate>.KeyCollection AspectTypes => Aspects.Keys;
        private static Dictionary<TypeReference, WeaveDelegate>.ValueCollection AspectMethods => Aspects.Values;

        private delegate void WeaveDelegate(WeaveArgs args);

        private const string WeaveMethodName = "WeaveAspect";

        private const string Postfix = ".pmgen";

        private static bool save;
        private static Queue<(string, string)> savedLibraries = new Queue<(string, string)>();

        public static void Run(string[] assemblyFiles, bool save = false)
        {
            Weaver.save = save;

            foreach (string fileName in assemblyFiles.Prepend(typeof(Weaver).Assembly.Location))
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
                ilProcessor ??= module.EntryPoint?.Body.GetILProcessor();
                Modules.Add(module);

                foreach (TypeDefinition typeDefinition in module.Types)
                {
                    if (!IsAspect(typeDefinition) || typeDefinition.IsAbstract)
                    {
                        continue;
                    }

                    MethodDefinition methodDefinition =
                        typeDefinition.Methods.SingleOrDefault(x => x.Name == WeaveMethodName);

                    if (methodDefinition == null)
                    {
                        TypeDefinition aspect = AspectTypes.FirstOrDefault(x => IsAssignableFrom(x, typeDefinition))!
                            .Resolve();

                        methodDefinition = aspect.Methods.SingleOrDefault(x => x.Name == WeaveMethodName);
                    }

                    if (methodDefinition == null)
                    {
                        break;
                    }

                    Aspects[typeDefinition] = MonoUtils.CreateDelegate<WeaveDelegate>(methodDefinition);
                    Console.WriteLine($"- Found aspect {typeDefinition.FullName}");
                }
            }

            Console.WriteLine();

            foreach (ModuleDefinition module in Modules)
            {
                ProcessModule(module);
            }

            while (Modules.Count > 0)
            {
                Modules[0].Dispose();
                Modules.RemoveAt(0);
            }

            while (savedLibraries.Count > 0)
            {
                (string moduleFile, string genFile) = savedLibraries.Dequeue();
                File.Replace(genFile, moduleFile, null);

                Console.WriteLine($"- Replaced {moduleFile} with {genFile}");
            }
        }

        private static void ProcessModule(ModuleDefinition module)
        {
            Console.WriteLine($"- Started processing {module.Name}");

            bool hasChanged = false;

            foreach (TypeDefinition type in module.GetTypes())
            {
                hasChanged |= ProcessMember(type);

                foreach (MethodDefinition methodDefinition in type.Methods)
                {
                    hasChanged |= ProcessMember(methodDefinition);
                }

                foreach (PropertyDefinition propertyDefinition in type.Properties)
                {
                    hasChanged |= ProcessMember(propertyDefinition);
                }

                foreach (EventDefinition eventDefinition in type.Events)
                {
                    hasChanged |= ProcessMember(eventDefinition);
                }

                foreach (FieldDefinition fieldDefinition in type.Fields)
                {
                    hasChanged |= ProcessMember(fieldDefinition);
                }
            }

            if (hasChanged && save)
            {
                string fileName = module.FileName;
                string path = fileName.Insert(fileName.LastIndexOf('.'), Postfix);
                module.Write(path);

                savedLibraries.Enqueue((fileName, path));

                Console.WriteLine($"-- Saved generated file to {path}");
            }

            Console.WriteLine($"- Finished processing {module.Name}\n");
        }

        private static bool IsAssignableFrom(TypeReference a, TypeReference b)
        {
            if (b == null)
            {
                return false;
            }

            bool result = a.FullName == b.FullName;

            if (result)
            {
                return true;
            }

            TypeDefinition type = Modules.SelectMany(x => x.GetTypes())
                .FirstOrDefault(typeDefinition => typeDefinition.FullName == b.FullName);

            if (type == null)
            {
                return false;
            }

            result |= IsAssignableFrom(a, type.BaseType);
            return result;
        }

        private static bool IsAspect(TypeReference typeReference)
        {
            return typeReference.Name.EndsWith("Aspect");
        }

        private static bool HasAspectAttribute(Collection<CustomAttribute> attributes)
        {
            return attributes.Select(x => x.Constructor.DeclaringType).Any(IsAspect);
        }

        private static bool ProcessMember(MemberReference member)
        {
            IMemberDefinition memberDefinition = member.Resolve();

            var attributes = memberDefinition.CustomAttributes;
            bool canProcess = HasAspectAttribute(attributes);

            if (canProcess)
            {
                Console.WriteLine($"-- Started processing {member.FullName}");

                WeaveArgs weaveArgs = new WeaveArgs(member.Module.Assembly,
                    member as TypeDefinition ?? member.DeclaringType.Resolve(),
                    ilProcessor.Body.Method, member as MethodDefinition);

                foreach (CustomAttribute attribute in attributes)
                {
                    TypeReference attributeType = attribute.Constructor.DeclaringType;
                    TypeReference aspect = AspectTypes.FirstOrDefault(x => x.FullName == attributeType.FullName);

                    if (aspect == null)
                    {
                        continue;
                    }

                    Aspects[aspect].Invoke(weaveArgs);
                    Console.WriteLine($"--- Processed {member.Name} with {aspect.FullName}");
                }

                Console.WriteLine($"-- Finished processing {member.FullName}");
            }

            return canProcess;
        }
    }
}