namespace Monkey.Core.Object;

public static class ObjectType
{
    public static string INTEGER_OBJ = "INTEGER";
    public static string BOOLEAN_OBJ = "BOOLEAN";
    public static string NULL_OBJ = "NULL";
    public static string RETURN_VALUE_OBJ = "RETURN_VALUE";
    public static string ERROR_OBJ = "ERROR";
    public static string FUNCTION_OBJ = "FUNCTION";
    public static string STRING_OBJ = "STRING";
    public static string BUILTIN_OBJ = "BUILTIN";
    public static string ARRAY_OBJ = "ARRAY";
    public static string HASH_OBJ = "HASH";
    public static string QUOTE_OBJ = "QUOTE";
    public static string MACRO_OBJ = "MACRO";
}

public abstract class Object
{
    public abstract string Type();
    public abstract string Inspect();

    public static bool operator ==(Object a, Object b)
    {
        if (a is null || b is null)
        {
            return false;
        }
        if (!a.Type().Equals(b.Type()))
        {
            return false;
        }

        if (a.Inspect().Equals(b.Inspect()))
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(Object a, Object b)
    {
        if (a is null || b is null)
        {
            return true;
        }
        if (!a.Type().Equals(b.Type()))
        {
            return true;
        }

        if (a.Inspect().Equals(b.Inspect()))
        {
            return false;
        }
        return true;
    }
}