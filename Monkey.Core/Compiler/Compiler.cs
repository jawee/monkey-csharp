using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Object;
using Boolean = Monkey.Core.AST.Boolean;

namespace Monkey.Core.Compiler;

public class Compiler
{
    private Instructions _instructions;
    private List<Object.Object> _constants;

    public Compiler()
    {
        _instructions = new();
        _constants = new();
    }

    public string? Compile(Node node)
    {
        if (node is Program p)
        {
            foreach (var s in p.Statements)
            {
                var err = Compile(s);
                if (err is not null)
                {
                    return err;
                }
            }
        }

        if (node is ExpressionStatement expr)
        {
            var err = Compile(expr.Expression);
            if (err is not null)
            {
                return err;
            }

            Emit(Opcode.OpPop);
        }

        if (node is InfixExpression infExpr)
        {
            string? err = null;

            if (infExpr.Operator.Equals("<"))
            {
                err = Compile(infExpr.Right);
                if (err is not null)
                {
                    return err;
                }

                err = Compile(infExpr.Left);
                if (err is not null)
                {
                    return err;
                }

                Emit(Opcode.OpGreaterThan);
                return null;
            }
            
            err = Compile(infExpr.Left);
            if (err is not null)
            {
                return err;
            }

            err = Compile(infExpr.Right);
            if (err is not null)
            {
                return err;
            }

            switch (infExpr.Operator)
            {
                case "+":
                    Emit(Opcode.OpAdd);
                    break;
                case "-":
                    Emit(Opcode.OpSub);
                    break;
                case "*":
                    Emit(Opcode.OpMul);
                    break;
                case "/":
                    Emit(Opcode.OpDiv);
                    break;
                case ">":
                    Emit(Opcode.OpGreaterThan);
                    break;
                case "==":
                    Emit(Opcode.OpEqual);
                    break;
                case "!=":
                    Emit(Opcode.OpNotEqual);
                    break;
                default:
                    return $"unknown operator {infExpr.Operator}";
                
            }
        }

        if (node is IntegerLiteral iLtrl)
        {
            var integer = new Integer {Value = iLtrl.Value};
            Emit(Opcode.OpConstant, new List<int>() {AddConstant(integer)});
        }
        
        if (node is Boolean b)
        {
            if (b.Value)
            {
                Emit(Opcode.OpTrue);
            }
            else
            {
                Emit(Opcode.OpFalse);
            }
        }

        if (node is PrefixExpression pExpr)
        {
            var err = Compile(pExpr.Right);
            if (err is not null)
            {
                return err;
            }

            switch (pExpr.Operator)
            {
               case "!":
                   Emit(Opcode.OpBang);
                   break;
               case "-":
                   Emit(Opcode.OpMinus);
                   break;
               default:
                   return $"unknown operator {pExpr.Operator}";
            }
        }
        
        return null;
    }

    private int AddConstant(Object.Object obj)
    {
        _constants.Add(obj);
        return _constants.Count - 1;
    }

    private int Emit(Opcode op, List<int>? operands = null)
    {
        var ins = Code.Code.Make(op, operands);
        var pos = AddInstruction(ins);
        return pos;
    }

    private int AddInstruction(Instructions ins)
    {
        var posNewInstruction = ins.Count;
        _instructions.AddRange(ins);
        return posNewInstruction;
    }

    public Bytecode Bytecode()
    {
        return new Bytecode
        {
            Instructions = _instructions,
            Constants = _constants
        };
    }
}