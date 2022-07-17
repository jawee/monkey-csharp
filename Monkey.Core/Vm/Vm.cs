using System.Reflection.Emit;
using Monkey.Core.Code;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Boolean = Monkey.Core.Object.Boolean;
using String = Monkey.Core.Object.String;

namespace Monkey.Core.Vm;

public class Vm
{
    private const int StackSize = 2048;
    private const int GlobalsSize = 65536;
    
    private readonly Boolean TRUE = new() {Value = true};
    private readonly Boolean FALSE = new() {Value = false};
    public static readonly Null NULL = new Null();
    public List<Object.Object> Constants { get; set; }
    public Instructions Instructions { get; set; }
    public Object.Object[] Stack { get; set; }
    public int sp { get; set; }

    private Object.Object[] Globals;

    public Vm(Bytecode bytecode)
    {
        Instructions = bytecode.Instructions;
        Constants = bytecode.Constants;

        Stack = new Object.Object[StackSize];
        sp = 0;

        Globals = new Object.Object[GlobalsSize];
    }

    public Vm(Bytecode bytecode, List<Object.Object> s) : this(bytecode)
    {
        Globals = s.ToArray();
    }

    public string? Run()
    {
        for (var ip = 0; ip < Instructions.Count; ip++)
        {
            var op = (Opcode) Instructions[ip];

            string? err = null;
            switch (op)
            {
                case Opcode.OpPop:
                    Pop();
                    break;
               case Opcode.OpAdd:
               case Opcode.OpSub:
               case Opcode.OpDiv:
               case Opcode.OpMul:
                   err = ExecuteBinaryOperation(op);
                   if (err is not null)
                   {
                       return err;
                   }
                    break;
               case Opcode.OpConstant:
                   var bytes = Instructions.GetRange(ip+1, Instructions.Count-ip-1);
                   var newInstr = new Instructions();
                   newInstr.AddRange(bytes);
                   var constIndex = Code.Code.ReadUint16(newInstr);
                   ip += 2;
                   err = Push(Constants[constIndex]);
                   if (err is not null)
                   {
                       return err;
                   }
                   break;
               case Opcode.OpTrue:
                   err = Push(TRUE);
                   if (err is not null)
                   {
                       return err;
                   }
                   break;
               case Opcode.OpFalse:
                   err = Push(FALSE);
                   if (err is not null)
                   {
                       return err;
                   }
                   break;
               case Opcode.OpEqual:
               case Opcode.OpNotEqual:
               case Opcode.OpGreaterThan:
                   err = ExecuteComparison(op);
                   if (err is not null)
                   {
                       return err;
                   }
                   break;
               case Opcode.OpBang:
                   err = ExecuteBangOperator();
                   if (err is not null)
                   {
                       return err;
                   }

                   break;
               case Opcode.OpMinus:
                   err = ExecuteMinusOperator();
                   if (err is not null)
                   {
                       return err;
                   }

                   break;
               case Opcode.OpJump:
                   var instr = new Instructions();
                   instr.AddRange(Instructions.GetRange(ip+1, Instructions.Count-ip-1));
                   var position = (int) Code.Code.ReadUint16(instr);
                   ip = position-1;

                   break;
               case Opcode.OpJumpNotTruthy:
                   var nInstr = new Instructions();
                   nInstr.AddRange(Instructions.GetRange(ip+1, Instructions.Count-ip-1));
                   var pos = (int) Code.Code.ReadUint16(nInstr);
                   ip += 2;
                   
                   var condition = Pop();
                   if (!IsTruthy(condition))
                   {
                       ip = pos - 1;
                   }
                   break;
                case Opcode.OpNull:
                    err = Push(NULL);
                    if (err is not null)
                    {
                        return err;
                    }

                    break;
                case Opcode.OpSetGlobal:
                    var inst = new Instructions();
                    var list = Instructions.GetRange(ip + 1, Instructions.Count - ip - 1);
                    inst.AddRange(list);
                    var globalIndex = Code.Code.ReadUint16(inst);
                    ip += 2;

                    Globals[globalIndex] = Pop();
                    break;
                case Opcode.OpGetGlobal:
                    var instructs = new Instructions();
                    var newlist = Instructions.GetRange(ip + 1, Instructions.Count - ip - 1);
                    instructs.AddRange(newlist);
                    var glblIdx = Code.Code.ReadUint16(instructs);
                    ip += 2;

                    err = Push(Globals[glblIdx]);
                    if (err is not null)
                    {
                        return err;
                    }
                    break;
                case Opcode.OpArray:
                    newlist = Instructions.GetRange(ip + 1, Instructions.Count - ip - 1);
                    inst = new Instructions();
                    inst.AddRange(newlist);
                    var numElements = Code.Code.ReadUint16(inst);
                    ip += 2;

                    var array = BuildArray(sp - numElements, sp);
                    sp = sp - numElements;

                    err = Push(array);
                    if (err is not null)
                    {
                        return err;
                    }
                    break;
            }
        }

        return null;
    }

    private Object.Object BuildArray(int startIndex, int endIndex)
    {
        var elements = new Object.Object[endIndex - startIndex];

        for (var i = startIndex; i < endIndex; i++)
        {
            elements[i - startIndex] = Stack[i];
        }

        return new Object.Array {Elements = elements.ToList()};
    }

    private static bool IsTruthy(Object.Object obj)
    {
        if (obj is Boolean b)
        {
            return b.Value;
        }

        if (obj is Null)
        {
            return false;
        }

        return true;
    }

    private string? ExecuteMinusOperator()
    {
        var operand = Pop();
        if (!operand.Type().Equals(ObjectType.INTEGER_OBJ))
        {
            return $"unsupported type for negation: {operand.Type()}";
        }

        var value = (operand as Integer).Value;
        return Push(new Integer {Value = -value});
    }

    private string? ExecuteBangOperator()
    {
        var operand = Pop();
        if (operand is Boolean b)
        {
            if (b.Value)
            {
                return Push(FALSE);
            }
            return Push(TRUE);
        }

        if (operand is Null n)
        {
            return Push(TRUE);
        }

        return Push(FALSE);
    }

    private string? ExecuteComparison(Opcode op)
    {
        var right = Pop();
        var left = Pop();

        if (left.Type().Equals(ObjectType.INTEGER_OBJ) && right.Type().Equals(ObjectType.INTEGER_OBJ))
        {
            return ExecuteIntegerComparison(op, left, right);
        }

        switch (op)
        {
            case Opcode.OpEqual:
                return Push(NativeBoolToBooleanObject(right == left));
            case Opcode.OpNotEqual:
                return Push(NativeBoolToBooleanObject(right != left));
            default:
                return $"unknown operator: {op} ({left.Type()} {right.Type()})";
        }
    }

    private Object.Object NativeBoolToBooleanObject(bool b)
    {
        if (b)
        {
            return TRUE;
        }

        return FALSE;
    }

    private string? ExecuteIntegerComparison(Opcode op, Object.Object left, Object.Object right)
    {
        var leftValue = (left as Integer).Value;
        var rightValue = (right as Integer).Value;

        switch (op)
        {
            case Opcode.OpEqual:
                return Push(NativeBoolToBooleanObject(rightValue == leftValue));
            case Opcode.OpNotEqual:
                return Push(NativeBoolToBooleanObject(rightValue != leftValue));
            case Opcode.OpGreaterThan:
                return Push(NativeBoolToBooleanObject(leftValue > rightValue));
            default:
                return $"unknown operator: {op}";
        }
    }

    private string? ExecuteBinaryOperation(Opcode op)
    {
        var right = Pop();
        var left = Pop();

        var leftType = left.Type();
        var rightType = right.Type();

        if (leftType.Equals(ObjectType.INTEGER_OBJ) && rightType.Equals(ObjectType.INTEGER_OBJ))
        {
            return ExecuteBinaryIntegerOperation(op, left, right);
        }

        if (leftType.Equals(ObjectType.STRING_OBJ) && rightType.Equals(ObjectType.STRING_OBJ))
        {
            return ExecuteBinaryStringOperation(op, left, right);
        }

        return $"unsupported types for binary operation: {leftType} {rightType}";
    }

    private string? ExecuteBinaryStringOperation(Opcode op, Object.Object left, Object.Object right)
    {
        if (op != Opcode.OpAdd)
        {
            return $"unknown string operator: {op}";
        }

        var leftValue = (left as String).Value;
        var rightValue = (right as String).Value;

        return Push(new String {Value = leftValue + rightValue});
    }

    private string? ExecuteBinaryIntegerOperation(Opcode op, Object.Object left, Object.Object right)
    {
        var leftValue = (left as Integer).Value;
        var rightValue = (right as Integer).Value;

        int result;

        switch (op)
        {
            case Opcode.OpAdd:
                result = leftValue + rightValue;
                break;
            case Opcode.OpSub:
                result = leftValue - rightValue;
                break;
            case Opcode.OpMul:
                result = leftValue * rightValue;
                break;
            case Opcode.OpDiv:
                result = leftValue / rightValue;
                break;
            default:
                return $"unknown integer operator {op}";
        }

        return Push(new Integer {Value = result});
    }

    public Object.Object LastPoppedStackElem()
    {
        return Stack[sp];
    }

    private Object.Object Pop()
    {
        var o = Stack[sp - 1];
        sp--;
        return o;
    }

    private string? Push(Object.Object o)
    {
        if (sp > StackSize)
        {
            return $"stack overflow";
        }

        Stack[sp] = o;
        sp++;
        
        return null;
    }

    public Object.Object? StackTop()
    {
        if (sp == 0)
        {
            return null;
        }

        return Stack[sp - 1];
    }
}