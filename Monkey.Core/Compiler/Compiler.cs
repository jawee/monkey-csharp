using System.Reflection.Emit;
using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Object;
using Boolean = Monkey.Core.AST.Boolean;

namespace Monkey.Core.Compiler;

public class Compiler
{
    private Instructions _instructions;
    private List<Object.Object> _constants;

    private EmittedInstruction _lastInstruction;
    private EmittedInstruction _previousInstruction;

    private SymbolTable _symbolTable;
    public Compiler()
    {
        _instructions = new();
        _constants = new();

        _lastInstruction = new EmittedInstruction();
        _previousInstruction = new EmittedInstruction();
        
        _symbolTable = new SymbolTable();
    }

    public Compiler(SymbolTable s, List<Object.Object> constants) : this()
    {
        _symbolTable = s;
        _constants = constants;
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

        if (node is IfExpression ifExpr)
        {
            var err = Compile(ifExpr.Condition);
            if (err is not null)
            {
                return err;
            }

            var jumpNotTruthyPos = Emit(Opcode.OpJumpNotTruthy, new List<int> {9999});

            err = Compile(ifExpr.Consequence);
            if (err is not null)
            {
                return err;
            }

            if (LastInstructionIsPop())
            {
                RemoveLastPop();
            }

            var jumpPos = Emit(Opcode.OpJump, new List<int> {9999});

            var afterConsequencePos = _instructions.Count;
            ChangeOperand(jumpNotTruthyPos, afterConsequencePos);
            
            if (ifExpr.Alternative is null)
            {
                Emit(Opcode.OpNull);
            }
            else
            {
                err = Compile(ifExpr.Alternative);
                if (err is not null)
                {
                    return err;
                }

                if (LastInstructionIsPop())
                {
                    RemoveLastPop();
                }
            }

            var afterAlternativePos = _instructions.Count;
            ChangeOperand(jumpPos, afterAlternativePos);
        }

        if (node is BlockStatement block)
        {
            for (var i = 0; i < block.Statements.Count; i++)
            {
                var s = block.Statements[i];
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

        if (node is LetStatement let)
        {
            var err = Compile(let.Value);
            if (err is not null)
            {
                return err;
            }

            var symbol = _symbolTable.Define(let.Name.Value);
            Emit(Opcode.OpSetGlobal, new List<int>{symbol.Index});
        }

        if (node is Identifier ident)
        {
            var (symbol, ok) = _symbolTable.Resolve(ident.Value);
            if (!ok)
            {
                return $"undefined variable {ident.Value}";
            }

            if (symbol != null) Emit(Opcode.OpGetGlobal, new List<int> {symbol.Value.Index});
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

    private void RemoveLastPop()
    {
        _instructions.RemoveRange(_lastInstruction.Position.Value, _instructions.Count-_lastInstruction.Position.Value);
        _lastInstruction = _previousInstruction;
    }

    private bool LastInstructionIsPop()
    {
        return _lastInstruction.Opcode == Opcode.OpPop;
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

        SetLastInstruction(op, pos);
        return pos;
    }

    private void SetLastInstruction(Opcode op, int pos)
    {
        var previous = _lastInstruction;
        var last = new EmittedInstruction {Opcode = op, Position = pos};

        _previousInstruction = previous;
        _lastInstruction = last;
    }

    private int AddInstruction(Instructions ins)
    {
        var posNewInstruction = _instructions.Count;
        _instructions.AddRange(ins);
        return posNewInstruction;
    }

    private void ReplaceInstructions(int pos, Instructions newInstruction)
    {
        for (var i = 0; i < newInstruction.Count; i++)
        {
            _instructions[pos + i] = newInstruction[i];
        }
    }

    private void ChangeOperand(int opPos, int operand)
    {
        var op = (Opcode) _instructions[opPos];
        var newInstruction = Code.Code.Make(op, new List<int> {operand});
        
        ReplaceInstructions(opPos, newInstruction);
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