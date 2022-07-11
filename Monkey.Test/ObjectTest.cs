using Monkey.Core.Object;
using NUnit.Framework;
using Boolean = Monkey.Core.Object.Boolean;

namespace Monkey.Test;

public class ObjectTest
{

    [Test]
    public void TestBooleanNotEquals()
    {
        var tests = new[]
        {
            new
            {
                Obj1 = new Boolean {Value = true},
                Obj2 = new Boolean {Value = true},
                Expected = false
            },
            new
            {
                Obj1 = new Boolean {Value = false},
                Obj2 = new Boolean {Value = false},
                Expected = false,
            },
            new
            {
                Obj1 = new Boolean {Value = true},
                Obj2 = new Boolean {Value = false},
                Expected = true
            }
        };

        foreach (var test in tests)
        {
            var result = test.Obj1 != test.Obj2;
            if (result != test.Expected)
            {
                Assert.Fail($"'{test.Obj1.Value}' != '{test.Obj2.Value}' should be '{test.Expected}', got '{result}'");
            }
        }
    }
    [Test]
    public void TestBooleanEquals()
    {
        var tests = new[]
        {
            new
            {
                Obj1 = new Boolean {Value = true},
                Obj2 = new Boolean {Value = true},
                Expected = true
            },
            new
            {
                Obj1 = new Boolean {Value = false},
                Obj2 = new Boolean {Value = false},
                Expected = true,
            },
            new
            {
                Obj1 = new Boolean {Value = true},
                Obj2 = new Boolean {Value = false},
                Expected = false
            }
        };

        foreach (var test in tests)
        {
            var result = test.Obj1 == test.Obj2;
            if (result != test.Expected)
            {
                Assert.Fail($"'{test.Obj1.Value}' == '{test.Obj2.Value}' should be '{test.Expected}', got '{result}'");
            }
        }
    }

    [Test]
    public void TestIntegerNotEquals()
    {
        var tests = new[]
        {
            new
            {
                Obj1 = new Integer {Value = 1},
                Obj2 = new Integer {Value = 1},
                Expected = false
            },
            new
            {
                Obj1 = new Integer {Value = 1},
                Obj2 = new Integer {Value = 2},
                Expected = true
            }
        };

        foreach (var test in tests)
        {
            var result = test.Obj1 != test.Obj2;
            if (result != test.Expected)
            {
                Assert.Fail($"'{test.Obj1.Value}' != '{test.Obj2.Value}' should be '{test.Expected}', got '{result}'");
            }
        }
    }
    [Test]
    public void TestIntegerEquals()
    {
        var tests = new[]
        {
            new
            {
                Obj1 = new Integer {Value = 1},
                Obj2 = new Integer {Value = 1},
                Expected = true
            },
            new
            {
                Obj1 = new Integer {Value = 1},
                Obj2 = new Integer {Value = 2},
                Expected = false
            }
        };

        foreach (var test in tests)
        {
            var result = test.Obj1 == test.Obj2;
            if (result != test.Expected)
            {
                Assert.Fail($"'{test.Obj1.Value}' == '{test.Obj2.Value}' should be '{test.Expected}', got '{result}'");
            }
        }
    }
}