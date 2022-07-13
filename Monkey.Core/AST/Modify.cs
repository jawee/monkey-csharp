namespace Monkey.Core.AST;

public class Modifier
{
    public static Node Modify(Node node, Func<Node, Node> modifier)
    {
        if (node is Program p)
        {
            for(var i = 0; i < p.Statements.Count; i++)
            {
                var val = p.Statements[i];
                var res = Modify(val, modifier);
                p.Statements[i] = res as Statement;
            }
        }

        if (node is ExpressionStatement e)
        {
            var res = Modify(e.Expression, modifier);
            e.Expression = res as Expression;
        }

        if (node is InfixExpression infix)
        {
            infix.Left = Modify(infix.Left, modifier) as Expression;
            infix.Right = Modify(infix.Right, modifier) as Expression;
        }

        if (node is PrefixExpression pref)
        {
            pref.Right = Modify(pref.Right, modifier) as Expression;
        }

        if (node is IndexExpression idx)
        {
            idx.Left = Modify(idx.Left, modifier) as Expression;
            idx.Index = Modify(idx.Index, modifier) as Expression;
        }

        if (node is IfExpression ifExpr)
        {
            ifExpr.Condition = Modify(ifExpr.Condition, modifier) as Expression;
            ifExpr.Consequence = Modify(ifExpr.Consequence, modifier) as BlockStatement;

            if (ifExpr.Alternative != null)
            {
                ifExpr.Alternative = Modify(ifExpr.Alternative, modifier) as BlockStatement;
            }
        }

        if (node is BlockStatement block)
        {
            for (var i = 0; i < block.Statements.Count; i++)
            {
                block.Statements[i] = Modify(block.Statements[i], modifier) as Statement;
            }
        }

        if (node is ReturnStatement ret)
        {
            ret.ReturnValue = Modify(ret.ReturnValue, modifier) as Expression;
        }

        if (node is LetStatement let)
        {
            let.Value = Modify(let.Value, modifier) as Expression;
        }

        if (node is FunctionLiteral func)
        {
            for (var i = 0; i < func.Parameters.Count; i++)
            {
                func.Parameters[i] = Modify(func.Parameters[i], modifier) as Identifier;
            }
            func.Body = Modify(func.Body, modifier) as BlockStatement;
        }

        if (node is ArrayLiteral arr)
        {
            for (var i = 0; i < arr.Elements.Count; i++)
            {
                arr.Elements[i] = Modify(arr.Elements[i], modifier) as Expression;
            }
        }

        if (node is HashLiteral hash)
        {
            var newPairs = new Dictionary<Expression, Expression>();
            foreach (var (k, v) in hash.Pairs)
            {
                var newKey = Modify(k, modifier) as Expression;
                var newVal = Modify(v, modifier) as Expression;
                newPairs.Add(newKey, newVal);
            }

            hash.Pairs = newPairs;
        }
        return modifier(node);
    }
}