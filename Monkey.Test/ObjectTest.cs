using Monkey.Core.Object;
using NUnit.Framework;
using Boolean = Monkey.Core.Object.Boolean;
using String = Monkey.Core.Object.String;

namespace Monkey.Test;

public class ObjectTest
{
    [Test]
    public void TestStringHashKey()
    {
        var hello1 = new String {Value = "Hello World"};
        var hello2 = new String {Value = "Hello World"};
        var diff1 = new String {Value = "My name is johnny"};
        var diff2 = new String {Value = "My name is johnny"};

        if (hello1.HashKey() != hello2.HashKey())
        {
            Assert.Fail($"strings with the same content have different hash keys");
        }

        if (diff1.HashKey() != diff2.HashKey())
        {
            Assert.Fail("strings with same content have different hash keys");
        }

        if (hello1.HashKey() == diff1.HashKey())
        {
            Assert.Fail("strings with different content have same hash keys");
        }
    }

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