using Monkey.Core.AST;
using Monkey.Core.Object;
using Array = Monkey.Core.Object.Array;
using Boolean = Monkey.Core.Object.Boolean;
using Environment = Monkey.Core.Object.Environment;
using String = Monkey.Core.Object.String;

namespace Monkey.Core;

public class Evaluator
{

    private static Dictionary<string, Object.Object> _builtins = new()
    {
        {"len", new Builtin { Fn = LenFunc }},
        {"first", new Builtin { Fn = FirstFunc}},
        {"last", new Builtin { Fn = LastFunc}},
        {"rest", new Builtin { Fn = RestFunc}},
        {"push", new Builtin { Fn = PushFunc}},
        {"puts", new Builtin { Fn = PutsFunc}}
    };

    private static Object.Object PutsFunc(List<Object.Object> args)
    {
        foreach (var arg in args)
        {
           Console.WriteLine(arg.Inspect()); 
        }

        return new Null();
    }

    private static Object.Object PushFunc(List<Object.Object> args)
    {
        
        if (args.Count != 2)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=2");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'last' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        var newElements = new List<Object.Object>();
        newElements.AddRange(arr.Elements);
        newElements.Add(args[1]);
        return new Array {Elements = newElements};
    }

    private static Object.Object RestFunc(List<Object.Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'last' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        if (length > 0)
        {
            var newList = new List<Object.Object>();
            newList.AddRange(arr.Elements.GetRange(1, length-1));
            return new Array {Elements = newList};
        }

        return new Null();
    }

    private static Object.Object LastFunc(List<Object.Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'last' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        if (length > 0)
        {
            return arr.Elements[length - 1];
        }

        return new Null();
    }

    private static Object.Object FirstFunc(List<Object.Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'first' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        if (arr.Elements.Count > 0)
        {
            return arr.Elements[0];
        }

        return new Null();
    }

    private static Object.Object LenFunc(List<Object.Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        var arg = args[0];
        if (arg is String str)
        {
            return new Integer {Value = str.Value.Length};
        }

        if (arg is Array arr)
        {
            return new Integer {Value = arr.Elements.Count};
        }
        return NewError($"argument to 'len' not supported, got {arg.Type()}");
    }


    public static Object.Object Eval(Node node, Environment env)
    {
        if (node is Program prog)
        {
            return EvalProgram(prog, env);
        }

        if (node is ExpressionStatement estmt)
        {
            return Eval(estmt.Expression, env);
        }

        if (node is PrefixExpression pExpr)
        {
            var right = Eval(pExpr.Right, env);
            if (IsError(right))
            {
                return right;
            }
            return EvalPrefixExpression(pExpr.Operator, right);
        }

        if (node is InfixExpression iExpr)
        {
            var left = Eval(iExpr.Left, env);
            if (IsError(left))
            {
                return left;
            }
            
            var right = Eval(iExpr.Right, env);
            if (IsError(right))
            {
                return right;
            }
            
            return EvalInfixExpression(iExpr.Operator, left, right);
        }

        if (node is HashLiteral hl)
        {
            return EvalHashLiteral(hl, env);
        }

        if (node is BlockStatement bStmt)
        {
            return EvalBlockStatement(bStmt, env);
        }

        if (node is IfExpression ifExpr)
        {
            return EvalIfExpression(ifExpr, env);
        }

        if (node is ReturnStatement rStmt)
        {
            var val = Eval(rStmt.ReturnValue, env);
            if (IsError(val))
            {
                return val;
            }
            return new ReturnValue {Value = val};
        }
        if (node is IntegerLiteral iNode)
        {
            return new Integer {Value = iNode.Value};
        }

        if (node is StringLiteral strNode)
        {
            return new Object.String {Value = strNode.Value};
        }

        if (node is AST.Boolean bNode)
        {
            return new Boolean {Value = bNode.Value};
        }

        if (node is LetStatement lStmt)
        {
            var val = Eval(lStmt.Value, env);
            if (IsError(val))
            {
                return val;
            }

            env.Set(lStmt.Name.Value, val);
        }

        if (node is FunctionLiteral fn)
        {
            var par = fn.Parameters;
            var body = fn.Body;

            return new Function {Parameters = par, Env = env, Body = body};
        }

        if (node is CallExpression ce)
        {
            var function = Eval(ce.Function, env);
            if (IsError(function))
            {
                return function;
            }

            var args = EvalExpressions(ce.Arguments, env);
            if (args.Count == 1 && IsError(args[0]))
            {
                return args[0];
            }

            return ApplyFunction(function, args);
        }

        if (node is ArrayLiteral arr)
        {
            var elements = EvalExpressions(arr.Elements, env);
            if (elements.Count == 1 && IsError(elements[0]))
            {
                return elements[0];
            }

            return new Object.Array { Elements = elements };
        }

        if (node is IndexExpression iExp)
        {
            var left = Eval(iExp.Left, env);
            if (IsError(left))
            {
                return left;
            }

            var index = Eval(iExp.Index, env);
            return EvalIndexExpression(left, index);
        }

        if (node is Identifier ident)
        {
            return EvalIdentifier(ident, env);
        }

        return new Null();
    }

    private static Object.Object EvalHashLiteral(HashLiteral node, Environment env)
    {
        var pairs = new Dictionary<HashKey, HashPair>();

        foreach (var (keyNode, valueNode) in node.Pairs)
        {
            var key = Eval(keyNode, env);
            if (IsError(key))
            {
                return key;
            }

            if (key is not Hashable)
            {
                return NewError($"unusable as hash key: {key.Type()}");
            }

            var hashKey = key as Hashable;

            var value = Eval(valueNode, env);
            if (IsError(value))
            {
                return value;
            }

            var hashed = hashKey.HashKey();
            pairs.Add(hashed, new HashPair {Key = key, Value = value});
        }

        return new Hash {Pairs = pairs};
    }

    private static Object.Object EvalIndexExpression(Object.Object left, Object.Object index)
    {
        if (left.Type() == ObjectType.ARRAY_OBJ && index.Type() == ObjectType.INTEGER_OBJ)
        {
            return EvalArrayIndexExpression(left, index);
        }

        if (left.Type() == ObjectType.HASH_OBJ)
        {
            return EvalHashIndexExpression(left, index);
        }

        return NewError($"index operator not supported: {left.Type()}");
    }

    private static Object.Object EvalHashIndexExpression(Object.Object hash, Object.Object index)
    {
        var hashObject = hash as Hash;

        if (index is not Hashable)
        {
            return NewError($"unusable as hash key: {index.Type()}");
        }

        var key = index as Hashable;

        if (!hashObject.Pairs.ContainsKey(key.HashKey()))
        {
            return new Null();
        }

        return hashObject.Pairs[key.HashKey()].Value;
    }

    private static Object.Object EvalArrayIndexExpression(Object.Object array, Object.Object index)
    {
        var arrayObj = array as Array;
        var idxObj = index as Integer;
        var idx = idxObj.Value;
        var max = arrayObj.Elements.Count - 1;

        if (idx < 0 || idx > max)
        {
            return new Null();
        }


        return arrayObj.Elements[idx];
    }

    private static Object.Object ApplyFunction(Object.Object function, List<Object.Object> args)
    {
        if (function is Function fn)
        {
            var extendedEnv = ExtendFunctionEnv(fn, args);
            var evaluated = Eval(fn.Body, extendedEnv);
            return UnwrapReturnValue(evaluated);
        }

        if (function is Builtin bn)
        {
            return bn.Fn(args);
        }

        return NewError($"not a function: {function.Type()}");
    }

    private static Object.Object UnwrapReturnValue(Object.Object obj)
    {
        if (obj is ReturnValue rv)
        {
            return rv.Value;
        }

        return obj;
    }

    private static Environment ExtendFunctionEnv(Function function, List<Object.Object> args)
    {
        var env = Environment.NewEnclosedEnvironment(function.Env);

        for (var i = 0; i < function.Parameters.Count; i++)
        {
            env.Set(function.Parameters[i].Value, args[i]);
        }

        return env;
    }

    private static List<Object.Object> EvalExpressions(List<Expression> exps, Environment env)
    {
        var result = new List<Object.Object>();

        foreach (var exp in exps)
        {
            var evaluated = Eval(exp, env);
            if (IsError(evaluated))
            {
                return new List<Object.Object> {evaluated};
            }
            result.Add(evaluated);
        }

        return result;
    }

    private static Object.Object EvalIdentifier(Identifier ident, Environment env)
    {
        var val = env.Get(ident.Value);
        if (val is not null)
        {
            return val;
        }

        if (_builtins.ContainsKey(ident.Value))
        {
            return _builtins[ident.Value];
        }

        return NewError($"identifier not found: {ident.Value}");
    }

    private static bool IsError(Object.Object obj)
    {
        if (obj != null)
        {
            return obj.Type().Equals(ObjectType.ERROR_OBJ);
        }

        return false;
    }


    private static Error NewError(string message)
    {
        return new Error {Message = message};
    }
    
    private static Object.Object EvalBlockStatement(BlockStatement block, Environment env)
    {
        Object.Object result = null;

        foreach (var statement in block.Statements)
        {
            result = Eval(statement, env);

            if (result != null)
            {
                
                if (result.Type().Equals(ObjectType.RETURN_VALUE_OBJ) || result.Type().Equals(ObjectType.ERROR_OBJ))
                {
                    return result;
                }
            }

        }

        return result;
    }

    private static Object.Object EvalProgram(Program program, Environment env)
    {
        Object.Object result = null;

        foreach (var statement in program.Statements)
        {
            result = Eval(statement, env);

            if (result is ReturnValue rVal)
            {
                return rVal.Value;
            }

            if (result is Error err)
            {
                return err;
            }
        }

        return result;
    }

    private static Object.Object EvalIfExpression(IfExpression ifExpr, Environment env)
    {
        var condition = Eval(ifExpr.Condition, env);
        if (IsError(condition))
        {
            return condition;
        }

        if (IsTruthy(condition))
        {
            return Eval(ifExpr.Consequence, env);
        }
        
        if (ifExpr.Alternative != null)
        {
            return Eval(ifExpr.Alternative, env);
        }
        
        return new Null();
    }

    private static bool IsTruthy(Object.Object obj)
    {
        if (obj is Null)
        {
            return false;
        }

        if (obj is Boolean b)
        {
            return b.Value;
        }

        return true;
    }

    private static Object.Object EvalInfixExpression(string op, Object.Object left, Object.Object right)
    {
        if (left.Type() == ObjectType.INTEGER_OBJ && right.Type() == ObjectType.INTEGER_OBJ)
        {
            return EvalIntegerInfixExpression(op, left, right);
        }

        if (left.Type() == ObjectType.STRING_OBJ && right.Type() == ObjectType.STRING_OBJ)
        {
            return EvalStringInfixExpression(op, left, right);
        }

        if (op.Equals("=="))
        {
            return NativeBoolToBooleanObject(left == right);
        }

        if (op.Equals("!="))
        {
            return NativeBoolToBooleanObject(left != right);
        }

        if (!left.Type().Equals(right.Type()))
        {
            return NewError($"type mismatch: {left.Type()} {op} {right.Type()}");
        }

        return NewError($"unknown operator: {left.Type()} {op} {right.Type()}");
    }

    private static Object.Object EvalStringInfixExpression(string op, Object.Object left, Object.Object right)
    {
        if (!op.Equals("+"))
        {
            return NewError($"unknown operator: {left.Type()} {op} {right.Type()}");
        }

        var leftStr = left as String;
        var rightStr = right as String;

        return new String {Value =  leftStr.Value + rightStr.Value};
    }

    private static Object.Object EvalIntegerInfixExpression(string op, Object.Object left, Object.Object right)
    {
        var leftObj = left as Integer;
        var rightObj = right as Integer;

        switch (op)
        {
            case "+":
                return new Integer {Value = leftObj.Value + rightObj.Value};
            case "-":
                return new Integer {Value = leftObj.Value - rightObj.Value};
            case "*":
                return new Integer {Value = leftObj.Value * rightObj.Value};
            case "/":
                return new Integer {Value = leftObj.Value / rightObj.Value};
            case "<":
                return NativeBoolToBooleanObject(leftObj.Value < rightObj.Value);
            case ">":
                return NativeBoolToBooleanObject(leftObj.Value > rightObj.Value);
            case "==":
                return NativeBoolToBooleanObject(leftObj.Value == rightObj.Value);
            case "!=":
                return NativeBoolToBooleanObject(leftObj.Value != rightObj.Value);
            default:
                return NewError($"unknown operator: {left.Type()} {op} {right.Type()}");
        }
    }

    private static Object.Object NativeBoolToBooleanObject(bool b)
    {
        if (b)
        {
            return new Boolean {Value = true};
        }

        return new Boolean {Value = false};
    }

    private static Object.Object EvalPrefixExpression(string op, Object.Object right)
    {
        switch (op)
        {
            case "!":
                return EvalBangOperatorExpression(right);
            case "-":
                return EvalMinusPrefixOperatorExpression(right);
            default:
                return NewError($"unknown operator: {op}{right.Type()}");
        }
    }

    private static Object.Object EvalMinusPrefixOperatorExpression(Object.Object right)
    {
        if (right.Type() != ObjectType.INTEGER_OBJ)
        {
            return NewError($"unknown operator: -{right.Type()}");
        }

        var value = right as Integer;

        return new Integer {Value = -value.Value};

    }

    private static Object.Object EvalBangOperatorExpression(Object.Object right)
    {
        var t = new Boolean {Value = true};
        var f = new Boolean {Value = false};
        var n = new Null();
        
        if (right is not Boolean)
        {
            return f;
        }
        
        var r = right as Boolean;
        
        if (r == null || r.Value == null)
        {
            return t;
        }
        
        if (r.Value == true)
        {
            return f;
        }

        if (r.Value == false)
        {
            return t;
        }


        return f;
    }

    private static Object.Object EvalStatements(List<Statement> progStatements, Environment env)
    {
        Object.Object result = null;

        foreach (var stmt in progStatements)
        {
            result = Eval(stmt, env);

            if (result is ReturnValue rVal)
            {
                return rVal.Value;
            }
        }

        return result;
    }
}