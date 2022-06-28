namespace Monkey.Core.AST;

public interface Expression
{
    public Node Node { get; set; }
    
    public abstract void ExpressionNode();
}