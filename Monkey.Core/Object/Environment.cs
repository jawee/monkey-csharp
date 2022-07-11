using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Environment
{
    private Dictionary<String, Object> _store = new Dictionary<string, Object>();

    private Environment _outer;
    public static Environment NewEnclosedEnvironment(Environment outer)
    {
        var env = new Environment
        {
            _outer = outer
        };
        return env;
    }

    public Object? Get(string name)
    {
        if (!_store.ContainsKey(name))
        {
            if (_outer == null)
            {
                return null;
            }
            var outerRes = _outer.Get(name);
            if (outerRes == null)
            {
                return null;
            }

            return outerRes;
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