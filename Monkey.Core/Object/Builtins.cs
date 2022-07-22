namespace Monkey.Core.Object;

public struct BuiltinsObj
{
    public string Name { get; set; }
    public Builtin Builtin { get; set; }
}
public class Builtins : List<BuiltinsObj>
{

    public Builtins()
    {
        Add(new() {Name = "len", Builtin = new Builtin {Fn = LenFunc}});
        Add(new() {Name = "puts", Builtin = new Builtin {Fn = PutsFunc}});
        Add(new() {Name = "first", Builtin = new Builtin {Fn = FirstFunc}});
        Add(new() {Name = "last", Builtin = new Builtin {Fn = LastFunc}});
        Add(new() {Name = "rest", Builtin = new Builtin {Fn = RestFunc}});
        Add(new() {Name = "push", Builtin = new Builtin {Fn = PushFunc}});
    }

    public Builtin? GetBuiltinByName(string name)
    {
        foreach (var def in this)
        {
            if (def.Name.Equals(name))
            {
                return def.Builtin;
            }
        }

        return null;
    }

    private static Object PushFunc(List<Object> args)
    {
        
        if (args.Count != 2)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=2");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'push' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        var newElements = new List<Object>();
        newElements.AddRange(arr.Elements);
        newElements.Add(args[1]);
        return new Array {Elements = newElements};
    }

    private static Object RestFunc(List<Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'last' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        if (length > 0)
        {
            var newList = new List<Object>();
            newList.AddRange(arr.Elements.GetRange(1, length-1));
            return new Array {Elements = newList};
        }

        return new Null();
    }

    private static Object LastFunc(List<Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'last' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        var length = arr.Elements.Count;
        if (length > 0)
        {
            return arr.Elements[length - 1];
        }

        return new Null();
    }

    private static Object FirstFunc(List<Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return NewError($"argument to 'first' must be ARRAY, got {args[0].Type()}");
        }

        var arr = args[0] as Array;
        if (arr.Elements.Count > 0)
        {
            return arr.Elements[0];
        }

        return new Null();
    }
    private static Object PutsFunc(List<Object> args)
    {
        foreach (var arg in args)
        {
           Console.WriteLine(arg.Inspect()); 
        }

        return new Null();
    }
    private static Object LenFunc(List<Object> args)
    {
        if (args.Count != 1)
        {
            return NewError($"wrong number of arguments. got={args.Count}, want=1");
        }

        var arg = args[0];
        if (arg is String str)
        {
            return new Integer {Value = str.Value.Length};
        }

        if (arg is Array arr)
        {
            return new Integer {Value = arr.Elements.Count};
        }
        return NewError($"argument to 'len' not supported, got {arg.Type()}");
    }

    private static Error NewError(string message)
    {
        return new Error {Message = message};
    }
}