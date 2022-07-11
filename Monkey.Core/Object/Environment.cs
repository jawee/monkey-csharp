using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Environment
{
    private Dictionary<String, Object> _store = new Dictionary<string, Object>();

    public Object? Get(string name)
    {
        if (!_store.ContainsKey(name))
        {
            return null;
        }

        var obj = _store[name];
        return obj;
    }

    public Object Set(string name, Object val)
    {
        _store.Add(name, val);
        return val;
    }
}