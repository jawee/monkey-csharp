using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monkey.Core.Compiler;

public class SymbolScope
{
    public static string GlobalScope = "GLOBAL";
    public static string LocalScope = "LOCAL";
    public static string BuiltinScope = "BUILTIN";
    public static string FreeScope = "FREE";
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
    public List<Symbol> FreeSymbols { get; set; }

    public SymbolTable()
    {
        _store = new Dictionary<string, Symbol>();
        FreeSymbols = new List<Symbol>();
        NumDefinitions = 0;
    }

    public SymbolTable(SymbolTable symbolTable) : this()
    {
        Outer = symbolTable;
    }

    public (Symbol?, bool) Resolve(string name)
    {
        if (_store.ContainsKey(name))
        {
            return (_store[name], true);
        }
        
        if (Outer is not null)
        {
            var (obj, ok) = Outer.Resolve(name);
            if (!ok)
            {
                return (obj, ok);
            }

            if (obj.Value.Scope.Equals(SymbolScope.GlobalScope) || obj.Value.Scope.Equals(SymbolScope.BuiltinScope))
            {
                return (obj, ok);
            }

            var free = DefineFree(obj.Value);
            return (free, true);
        }

        return (null, false);

    }

    private Symbol DefineFree(Symbol original)
    {
        FreeSymbols.Add(original);

        var symbol = new Symbol {Name = original.Name, Index = FreeSymbols.Count - 1};
        symbol.Scope = SymbolScope.FreeScope;

        _store[original.Name] = symbol;

        return symbol;
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
