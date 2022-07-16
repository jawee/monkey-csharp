namespace Monkey.Core.Compiler;

public class SymbolScope
{
    public static string GlobalScope = "GLOBAL";
}
public struct Symbol
{
    public string Name { get; set; }
    public string Scope { get; set; }
    public int Index { get; set; }
}

public class SymbolTable
{

    private Dictionary<string, Symbol> _store;
    private int _numDefinitions;

    public SymbolTable()
    {
        _store = new Dictionary<string, Symbol>();
        _numDefinitions = 0;
    }

    public (Symbol? result, bool ok) Resolve(string name)
    {
        if (!_store.ContainsKey(name))
        {
            return (null, false);
        }

        return (_store[name], true);
    }

    public Symbol Define(string name)
    {
        var symbol = new Symbol {Name = name, Index = _numDefinitions, Scope = SymbolScope.GlobalScope};
        _store[name] = symbol;
        _numDefinitions++;
        return symbol;
    }
}