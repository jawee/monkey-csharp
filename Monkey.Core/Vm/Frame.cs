using Monkey.Core.Code;
using Monkey.Core.Object;

namespace Monkey.Core.Vm;

public class Frame
{
    public CompiledFunction Fn { get; set; }
    public int Ip { get; set; }
    public int BasePointer { get; set; }

    public Frame(CompiledFunction fn, int basePointer)
    {
        Fn = fn;
        Ip = -1;
        BasePointer = basePointer;
    }

    public Instructions Instructions()
    {
        return Fn.Instructions;
    }
    
}