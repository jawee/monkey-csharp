using System.Diagnostics.SymbolStore;
using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Object;
using Boolean = Monkey.Core.AST.Boolean;
using String = Monkey.Core.Object.String;

namespace Monkey.Core.Compiler;

public class Compiler
{
    private Instructions _instructions;
    private List<Object.Object> _constants;

    private EmittedInstruction _lastInstruction;
    private EmittedInstruction _previousInstruction;

    public SymbolTable SymbolTable { get; set; }

    public int ScopeIndex { get; set; }
    public List<CompilationScope> Scopes { get; set; }
    public Compiler()
    {
        _instructions = new();
        _constants = new();

        _lastInstruction = new EmittedInstruction();
        _previousInstruction = new EmittedInstruction();
        
        SymbolTable = new SymbolTable();

        var builtins = new Builtins();
        for(var i = 0; i < builtins.Count; i++)
        {
            var k = builtins[i];
            SymbolTable.DefineBuiltin(i, k.Name);
        }

        var mainScope = new CompilationScope
        {
            Instructions = _instructions,
            LastInstruction = new(),
            PreviousInstruction = new()
        };
        Scopes = new() {mainScope};
        ScopeIndex = 0;
    }

    public Compiler(SymbolTable s, List<Object.Object> constants) : this()
    {
        SymbolTable = s;
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

            if (LastInstructionIs(Opcode.OpPop))
            {
                RemoveLastPop();
            }

            var jumpPos = Emit(Opcode.OpJump, new List<int> {9999});

            var afterConsequencePos = CurrentInstructions().Count;
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

                if (LastInstructionIs(Opcode.OpPop))
                {
                    RemoveLastPop();
                }
            }

            var afterAlternativePos = CurrentInstructions().Count;
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

            var symbol = SymbolTable.Define(let.Name.Value);
            if (symbol.Scope.Equals(SymbolScope.GlobalScope))
            {
                Emit(Opcode.OpSetGlobal, new List<int>{symbol.Index});
            }
            else
            {
                Emit(Opcode.OpSetLocal, new() {symbol.Index});
            }
        }

        if (node is Identifier ident)
        {
            var (symbol, ok) = SymbolTable.Resolve(ident.Value);
            if (!ok)
            {
                return $"undefined variable {ident.Value}";
            }
            
            LoadSymbol(symbol.Value);
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

        if (node is StringLiteral strLit)
        {
            var str = new String {Value = strLit.Value};
            Emit(Opcode.OpConstant, new List<int> {AddConstant(str)});
        }

        if (node is ArrayLiteral arrLit)
        {
            foreach (var el in arrLit.Elements)
            {
                var err = Compile(el);
                if (err is not null)
                {
                    return err;
                }
            }

            Emit(Opcode.OpArray, new() {arrLit.Elements.Count});
        }

        if (node is HashLiteral has)
        {
            var keys = new List<Expression>();
            foreach (var (k, v) in has.Pairs)
            {
                keys.Add(k);
            }

            keys = keys.OrderBy(x => x.String()).ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                var err = Compile(k);
                if (err is not null)
                {
                    return err;
                }

                err = Compile(has.Pairs[k]);
                if (err is not null)
                {
                    return err;
                }
            }

            Emit(Opcode.OpHash, new() {has.Pairs.Count * 2});
        }

        if (node is IndexExpression indExpr)
        {
            var err = Compile(indExpr.Left);
            if (err is not null)
            {
                return err;
            }

            err = Compile(indExpr.Index);
            if (err is not null)
            {
                return err;
            }

            Emit(Opcode.OpIndex);
        }

        if (node is FunctionLiteral func)
        {
            EnterScope();
            foreach (var param in func.Parameters)
            {
                SymbolTable.Define(param.Value);
            }
            var err = Compile(func.Body);
            if (err is not null)
            {
                return err;
            }

            if (LastInstructionIs(Opcode.OpPop))
            {
                ReplaceLastPopWithReturn();
            }

            if (!LastInstructionIs(Opcode.OpReturnValue))
            {
                Emit(Opcode.OpReturn);
            }

            var numLocals = SymbolTable.NumDefinitions;
            var instructions = LeaveScope();

            var compiledFn = new CompiledFunction {Instructions = instructions, NumLocals = numLocals, NumParameters = func.Parameters.Count};
            Emit(Opcode.OpConstant, new() {AddConstant(compiledFn)});
        }

        if (node is ReturnStatement ret)
        {
            var err = Compile(ret.ReturnValue);
            if (err is not null)
            {
                return err;
            }

            Emit(Opcode.OpReturnValue);
        }

        if (node is CallExpression cexpr)
        {
            var err = Compile(cexpr.Function);
            if (err is not null)
            {
                return err;
            }

            foreach (var a in cexpr.Arguments)
            {
                err = Compile(a);
                if (err is not null)
                {
                    return err;
                }
            }

            Emit(Opcode.OpCall, new() {cexpr.Arguments.Count});
        }
        
        return null;
    }

    private void ReplaceLastPopWithReturn()
    {
        var lastPos = Scopes[ScopeIndex].LastInstruction.Position;
        ReplaceInstructions(lastPos.Value, Code.Code.Make(Opcode.OpReturnValue));

        Scopes[ScopeIndex].LastInstruction.Opcode = Opcode.OpReturnValue;
    }

    private void LoadSymbol(Symbol s)
    {
        if (s.Scope.Equals(SymbolScope.GlobalScope))
        {
            Emit(Opcode.OpGetGlobal, new() {s.Index});
            return;
        }

        if (s.Scope.Equals(SymbolScope.LocalScope))
        {
            Emit(Opcode.OpGetLocal, new() {s.Index});
            return;
        }

        if (s.Scope.Equals(SymbolScope.BuiltinScope))
        {
            Emit(Opcode.OpGetBuiltin, new() {s.Index});
        }
    }

    private void RemoveLastPop()
    {
        var last = Scopes[ScopeIndex].LastInstruction;
        var previous = Scopes[ScopeIndex].PreviousInstruction;

        var old = CurrentInstructions();
        old.RemoveRange(last.Position.Value, old.Count-last.Position.Value);

        Scopes[ScopeIndex].Instructions = old;
        Scopes[ScopeIndex].LastInstruction = previous;
    }

    private bool LastInstructionIs(Opcode op)
    {
        if (CurrentInstructions().Count == 0)
        {
            return false;
        }
        return Scopes[ScopeIndex].LastInstruction.Opcode == op;
    }

    private int AddConstant(Object.Object obj)
    {
        _constants.Add(obj);
        return _constants.Count - 1;
    }

    public int Emit(Opcode op, List<int>? operands = null)
    {
        var ins = Code.Code.Make(op, operands);
        var pos = AddInstruction(ins);

        SetLastInstruction(op, pos);
        return pos;
    }

    private void SetLastInstruction(Opcode op, int pos)
    {
        var previous = Scopes[ScopeIndex].LastInstruction;
        var last = new EmittedInstruction {Opcode = op, Position = pos};

        Scopes[ScopeIndex].PreviousInstruction = previous;
        Scopes[ScopeIndex].LastInstruction = last;
    }

    private void ReplaceInstructions(int pos, Instructions newInstruction)
    {
        var ins = CurrentInstructions();
        for (var i = 0; i < newInstruction.Count; i++)
        {
            ins[pos + i] = newInstruction[i];
        }
    }

    private void ChangeOperand(int opPos, int operand)
    {
        var op = (Opcode) CurrentInstructions()[opPos];
        var newInstruction = Code.Code.Make(op, new List<int> {operand});
        
        ReplaceInstructions(opPos, newInstruction);
    }

    public Bytecode Bytecode()
    {
        return new Bytecode
        {
            Instructions = CurrentInstructions(),
            Constants = _constants
        };
    }

    private Instructions CurrentInstructions()
    {
        return Scopes[ScopeIndex].Instructions;
    }

    private int AddInstruction(Instructions ins)
    {
        var curInstr = CurrentInstructions();
        var posNewInstruction = curInstr.Count;
        curInstr.AddRange(ins);
        var updatedInstructions = curInstr;

        Scopes[ScopeIndex].Instructions = updatedInstructions;

        return posNewInstruction;
    }
    
    public void EnterScope()
    {
        var scope = new CompilationScope
        {
            Instructions = new(),
            LastInstruction = new(),
            PreviousInstruction = new()
        };
        
        Scopes.Add(scope);
        ScopeIndex++;

        SymbolTable = new SymbolTable(SymbolTable);
    }

    public Instructions LeaveScope()
    {
        var instructions = CurrentInstructions();

        Scopes.RemoveAt(Scopes.Count-1);
        ScopeIndex--;

        SymbolTable = SymbolTable.Outer;
        
        return instructions;
    }
}