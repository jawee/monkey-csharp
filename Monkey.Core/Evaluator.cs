using System.Diagnostics;
using Monkey.Core.AST;
using Monkey.Core.Object;
using Boolean = Monkey.Core.Object.Boolean;

namespace Monkey.Core;

public class Evaluator
{
    public static Object.Object Eval(Node node)
    {
        if (node is Program prog)
        {
            return EvalStatements(prog.Statements);
        }

        if (node is ExpressionStatement estmt)
        {
            return Eval(estmt.Expression);
        }

        if (node is PrefixExpression pExpr)
        {
            var right = Eval(pExpr.Right);
            return EvalPrefixExpression(pExpr.Operator, right);
        }

        if (node is InfixExpression iExpr)
        {
            var left = Eval(iExpr.Left);
            var right = Eval(iExpr.Right);
            return EvalInfixExpression(iExpr.Operator, left, right);
        }

        if (node is BlockStatement bStmt)
        {
            return EvalStatements(bStmt.Statements);
        }

        if (node is IfExpression ifExpr)
        {
            return EvalIfExpression(ifExpr);
        }
        if (node is IntegerLiteral iNode)
        {
            return new Integer {Value = iNode.Value};
        }

        if (node is AST.Boolean bNode)
        {
            return new Boolean {Value = bNode.Value};
        }

        return null;
    }

    private static Object.Object EvalIfExpression(IfExpression ifExpr)
    {
        var condition = Eval(ifExpr.Condition);

        if (IsTruthy(condition))
        {
            return Eval(ifExpr.Consequence);
        }
        
        if (ifExpr.Alternative != null)
        {
            return Eval(ifExpr.Alternative);
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

        return new Null();
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
                return new Null();
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
                return new Null();
        }
    }

    private static Object.Object EvalMinusPrefixOperatorExpression(Object.Object right)
    {
        if (right.Type() != ObjectType.INTEGER_OBJ)
        {
            return new Null();
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

    private static Object.Object EvalStatements(List<Statement> progStatements)
    {
        Object.Object result = null;

        foreach (var stmt in progStatements)
        {
            result = Eval(stmt);
        }

        return result;
    }
}