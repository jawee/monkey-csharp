using Monkey.Core.AST;
using Monkey.Core.Object;
using Boolean = Monkey.Core.Object.Boolean;
using Environment = Monkey.Core.Object.Environment;

namespace Monkey.Core;

public class Evaluator
{
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

        if (node is Identifier ident)
        {
            return EvalIdentifier(ident, env);
        }

        return null;
    }

    private static Object.Object EvalIdentifier(Identifier ident, Environment env)
    {
        var val = env.Get(ident.Value);
        if (val is null)
        {
            return NewError($"identifier not found: {ident.Value}");
        }

        return val;
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