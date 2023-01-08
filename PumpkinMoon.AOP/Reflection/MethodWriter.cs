using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.AOP.Reflection
{
    internal class MethodWriter : IDisposable
    {
        private BufferWriter writer;

        public byte[] GetIlByteArray()
        {
            return writer.ToArray();
        }

        public void WriteInstruction(Mono.Cecil.Cil.Instruction instruction)
        {
            OpCode opCode = instruction.OpCode;

            if (opCode.Size == 1)
            {
                writer.WriteUnmanaged(opCode.Op2);
            }
            else
            {
                writer.WriteUnmanaged(opCode.Op1);
                writer.WriteUnmanaged(opCode.Op2);
            }

            if (opCode.OperandType == OperandType.InlineNone)
            {
                return;
            }

            object operand = instruction.Operand;

            switch (opCode.OperandType)
            {
                case OperandType.InlineSwitch:
                {
                    int[] targets = (int[])operand;
                    writer.WriteUnmanaged(targets.Length);

                    for (int i = 0; i < targets.Length; i++)
                    {
                        writer.WriteUnmanaged(targets[i]);
                    }

                    break;
                }
                case OperandType.ShortInlineBrTarget:
                {
                    sbyte value = (sbyte)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineBrTarget:
                {
                    int value = (int)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.ShortInlineVar:
                {
                    byte value = (byte)((VariableDefinition)operand).Index;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.ShortInlineArg:
                {
                    byte value = (byte)((ParameterDefinition)operand).Index;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineVar:
                {
                    short value = (short)((VariableDefinition)operand).Index;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineArg:
                {
                    short value = (short)((ParameterDefinition)operand).Index;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineSig:
                {
                    int value = ((CallSite)operand).MetadataToken.ToInt32();
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.ShortInlineI:
                {
                    if (opCode == OpCodes.Ldc_I4_S)
                    {
                        sbyte value = (sbyte)operand;
                        writer.WriteUnmanaged(value);
                    }
                    else
                    {
                        byte value = (byte)operand;
                        writer.WriteUnmanaged(value);
                    }

                    break;
                }
                case OperandType.InlineI:
                {
                    int value = (int)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineI8:
                {
                    long value = (long)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.ShortInlineR:
                {
                    float value = (float)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineR:
                {
                    double value = (double)operand;
                    writer.WriteUnmanaged(value);
                    break;
                }
                case OperandType.InlineString:
                {
                    throw new NotImplementedException();
                }
                case OperandType.InlineType:
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                {
                    int value = ((IMetadataTokenProvider)operand).MetadataToken.ToInt32();
                    writer.WriteUnmanaged(value);
                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}