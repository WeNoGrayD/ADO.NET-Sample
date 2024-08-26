using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmClassLib
{
    public class Department : IStringRepresentable, IDisassembleable
    {
        public int Id { get; set; }

        protected string _name;

        public virtual string Name { get => _name; set => _name = value; }

        public Department() { return; }

        public Department(string name)
        {
            Name = name;

            return;
        }

        public Department(int id, string name) : this(name)
        {
            Id = id;

            return;
        }

        public Department(int id) : this(id, "")
        {
            return;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public string Repr()
        {
            return $"(N'{Name}')";
        }

        public IEnumerable<object> Disassemble()
        {
            return new[] { Name };
        }
    }
}
