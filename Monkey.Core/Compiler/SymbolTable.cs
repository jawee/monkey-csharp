using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monkey.Core.Compiler;

public class SymbolScope
{
    public static string GlobalScope = "GLOBAL";
    public static string LocalScope = "LOCAL";
    public static string BuiltinScope = "BUILTIN";
}
public struct Symbol
{
    public string Name { get; set; }
    public string Scope { get; set; }
    public int Index { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Symbol s)
        {
            return false;
        }

        return ToString().Equals(s.ToString());
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class SymbolTable
{
    public SymbolTable? Outer { get; set; }

    private Dictionary<string, Symbol> _store;
    public int NumDefinitions { get; set; }

    public SymbolTable()
    {
        _store = new Dictionary<string, Symbol>();
        NumDefinitions = 0;
    }

    public SymbolTable(SymbolTable symbolTable) : this()
    {
        Outer = symbolTable;
    }

    public (Symbol?, bool) Resolve(string name)
    {
        if (!_store.ContainsKey(name) && Outer is not null)
        {
            var (obj, ok) = Outer.Resolve(name);
            return (obj, ok);
        }

        return (_store[name], true);
    }

    public Symbol Define(string name)
    {
        var symbol = new Symbol {Name = name, Index = NumDefinitions};
        if (Outer == null)
        {
            symbol.Scope = SymbolScope.GlobalScope;
        }
        else
        {
            symbol.Scope = SymbolScope.LocalScope;
        }
        _store[name] = symbol;
        NumDefinitions++;
        return symbol;
    }

    public Symbol DefineBuiltin(int index, string name)
    {
        var symbol = new Symbol {Name = name, Index = index, Scope = SymbolScope.BuiltinScope};
        _store[name] = symbol;

        return symbol;
    }
    
}
