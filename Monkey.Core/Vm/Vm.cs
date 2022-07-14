using Monkey.Core.Code;
using Monkey.Core.Compiler;
using Monkey.Core.Object;

namespace Monkey.Core.Vm;

public class Vm
{
    private const int StackSize = 2048;
    public List<Object.Object> Constants { get; set; }
    public Instructions Instructions { get; set; }
    public Object.Object[] Stack { get; set; }
    public int sp { get; set; }

    public Vm(Bytecode bytecode)
    {
        Instructions = bytecode.Instructions;
        Constants = bytecode.Constants;

        Stack = new Object.Object[StackSize];
        sp = 0;
    }

    public string? Run()
    {
        for (var ip = 0; ip < Instructions.Count; ip++)
        {
            var op = (Opcode) Instructions[ip];

            switch (op)
            {
               case Opcode.OpAdd:
                    var right = Pop();
                    var left = Pop();
                    var leftValue = (left as Integer).Value;
                    var rightValue = (right as Integer).Value;

                    var result = leftValue + rightValue;
                    Push(new Integer {Value = result});
                    break;
               case Opcode.OpConstant:
                   var bytes = Instructions.GetRange(ip+1, Instructions.Count - ip - 1);
                   var newInstr = new Instructions();
                   newInstr.AddRange(bytes);
                   var constIndex = Code.Code.ReadUint16(newInstr);
                   ip += 2;
                   var err = Push(Constants[constIndex]);
                   if (err is not null)
                   {
                       return err;
                   }
                   break;
            }
        }

        return null;
    }

    private Object.Object Pop()
    {
        var o = Stack[sp - 1];
        sp--;
        return o;
    }

    private string? Push(Object.Object o)
    {
        if (sp > StackSize)
        {
            return $"stack overflow";
        }

        Stack[sp] = o;
        sp++;
        
        return null;
    }

    public Object.Object? StackTop()
    {
        if (sp == 0)
        {
            return null;
        }

        return Stack[sp - 1];
    }
}