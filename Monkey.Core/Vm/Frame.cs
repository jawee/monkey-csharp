using Monkey.Core.Code;
using Monkey.Core.Object;

namespace Monkey.Core.Vm;

public class Frame
{
    public Closure Cl { get; set; }
    public int Ip { get; set; }
    public int BasePointer { get; set; }

    public Frame(Closure cl, int basePointer)
    {
        Cl = cl;
        Ip = -1;
        BasePointer = basePointer;
    }

    public Instructions Instructions()
    {
        return Cl.Fn.Instructions;
    }
    
}