namespace Monkey.Core.AST;

public interface Expression : Node
{
    public Node Node { get; set; }
    public abstract void ExpressionNode();
    public abstract string String();
}