using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PumpkinMoon.AOP.Reflection
{
    internal readonly struct Instruction
    {
        public OpCode OpCode { get; }
        public object Operand { get; }

        public Instruction(OpCode opCode, long operand) : this()
        {
            OpCode = opCode;
            Operand = Resolve(operand) ?? operand;
        }

        public Instruction(OpCode opCode, object operand)
        {
            OpCode = opCode;
            Operand = operand;
        }

        private object Resolve(long operand)
        {
            if (OpCode.OperandType == OperandType.InlineNone)
            {
                return null;
            }

            int token = (int)operand;

            Module module = Assembly.GetExecutingAssembly().Modules.First();

            try
            {
                switch (OpCode.OperandType)
                {
                    case OperandType.InlineString:
                    {
                        return module.ResolveString(token);
                    }
                    case OperandType.InlineType:
                    {
                        return module.ResolveType(token);
                    }
                    case OperandType.InlineField:
                    {
                        return module.ResolveField(token);
                    }
                    case OperandType.InlineMethod:
                    {
                        return module.ResolveMethod(token);
                    }
                    case OperandType.InlineTok:
                    {
                        return module.ResolveMember(token);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return Operand;
        }

        public override string ToString()
        {
            if (OpCode.OperandType != OperandType.InlineNone)
            {
                return $"{OpCode.Name} {Operand}";
            }

            return $"{OpCode.Name}";
        }
    }
}