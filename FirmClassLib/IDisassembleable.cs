using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmClassLib
{
    public interface IDisassembleable
    {
        int Id { get; }

        IEnumerable<object> Disassemble();
    }
}
