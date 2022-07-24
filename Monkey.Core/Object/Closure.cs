namespace Monkey.Core.Object;

public class Closure : Object
{
    public CompiledFunction Fn { get; set; }
    public List<Object> Free { get; set; }
    public override string Type()
    {
        return ObjectType.CLOSURE_OBJ;
    }

    public override string Inspect()
    {
        return $"Closure{this}";
    }
}