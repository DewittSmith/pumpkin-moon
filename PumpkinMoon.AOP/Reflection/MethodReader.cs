using System;
using System.Reflection.Emit;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.AOP.Reflection
{
    internal class MethodReader : IDisposable
    {
        private static readonly OpCode[] OneByteOpCode = new OpCode[0x100];
        private static readonly OpCode[] TwoByteOpCode = new OpCode[0x100];

        private static bool initializedCodes;

        private static void InitializeCodes()
        {
            if (initializedCodes)
            {
                return;
            }

            initializedCodes = true;

            var opCodes = Array.ConvertAll(typeof(OpCodes).GetFields(), input => (OpCode)input.GetValue(null));

            foreach (OpCode opCode in opCodes)
            {
                ushort value = (ushort)opCode.Value;

                if (opCode.Size == 1)
                {
                    OneByteOpCode[value] = opCode;
                }
                else
                {
                    TwoByteOpCode[value & 0xff] = opCode;
                }
            }
        }

        private BufferReader reader;

        public MethodReader(byte[] ilBytes)
        {
            InitializeCodes();
            reader = new BufferReader(ilBytes);
        }

        public bool TryReadInstruction(out Instruction instruction)
        {
            if (!reader.CanRead)
            {
                instruction = default;
                return false;
            }

            OpCode opCode;
            reader.ReadUnmanaged(out byte value);

            if (value != 0xfe)
            {
                opCode = OneByteOpCode[value];
            }
            else
            {
                reader.ReadUnmanaged(out value);
                opCode = TwoByteOpCode[value];
            }

            long operand;
            switch (opCode.OperandType)
            {
                case OperandType.InlineSwitch:
                {
                    reader.ReadUnmanaged(out int length);
                    int baseOffset = reader.Position + 4 * length;
                    int[] branches = new int [length];
                    for (int i = 0; i < length; i++)
                    {
                        reader.ReadUnmanaged(out int op);
                        branches[i] = baseOffset + op;
                    }

                    operand = branches.GetHashCode();
                    break;
                }
                case OperandType.ShortInlineBrTarget:
                {
                    reader.ReadUnmanaged(out sbyte sb);
                    operand = sb + reader.Position;
                    break;
                }
                case OperandType.InlineBrTarget:
                {
                    reader.ReadUnmanaged(out int sb);
                    operand = sb + reader.Position;
                    break;
                }
                case OperandType.ShortInlineI:
                {
                    if (opCode == OpCodes.Ldc_I4_S)
                    {
                        reader.ReadUnmanaged(out sbyte sb);
                        operand = sb;
                    }
                    else
                    {
                        reader.ReadUnmanaged(out byte sb);
                        operand = sb;
                    }

                    break;
                }
                case OperandType.InlineI:
                {
                    reader.ReadUnmanaged(out int sb);
                    operand = sb;
                    break;
                }
                case OperandType.ShortInlineR:
                {
                    reader.ReadUnmanaged(out int sb);
                    operand = sb;
                    break;
                }
                case OperandType.InlineR:
                {
                    reader.ReadUnmanaged(out operand);
                    break;
                }
                case OperandType.InlineI8:
                {
                    reader.ReadUnmanaged(out operand);
                    break;
                }
                case OperandType.ShortInlineVar:
                {
                    reader.ReadUnmanaged(out byte sb);
                    operand = sb;
                    break;
                }
                case OperandType.InlineVar:
                {
                    reader.ReadUnmanaged(out ushort sb);
                    operand = sb;
                    break;
                }
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                {
                    reader.ReadUnmanaged(out int token);
                    operand = token;
                    break;
                }
                case OperandType.InlineNone:
                {
                    operand = 0;
                    break;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }

            instruction = new Instruction(opCode, operand);
            return true;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}