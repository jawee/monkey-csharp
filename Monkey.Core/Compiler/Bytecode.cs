using Monkey.Core.Code;
using Monkey.Core.Object;

namespace Monkey.Core.Compiler;

public class Bytecode
{
    public Instructions Instructions { get; set; }
    public List<Object.Object> Constants { get; set; }
}