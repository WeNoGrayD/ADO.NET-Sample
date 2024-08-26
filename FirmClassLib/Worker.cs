using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FirmClassLib
{
    public class Worker : IStringRepresentable, IDisassembleable
    {
        public int Id { get; set; }

        protected string _firstName;

        public virtual string FirstName 
        { 
            get => _firstName; 
            set => _firstName = value; 
        }

        protected string _lastName;

        public virtual string LastName
        {
            get => _lastName;
            set => _lastName = value;
        }

        protected string _patronymic;

        public virtual string Patronymic
        {
            get => _patronymic;
            set => _patronymic = value;
        }

        protected string _position;

        public virtual string Position
        {
            get => _position;
            set => _position = value;
        }

        protected decimal _salary;

        public virtual decimal Salary
        {
            get => _salary;
            set => _salary = value;
        }

        public Worker() { return; }

        public Worker(
            string firstName, 
            string lastName, 
            string patronymic,
            string position,
            decimal salary)
        {
            FirstName = firstName;
            LastName = lastName;
            Patronymic = patronymic;
            Position = position;
            Salary = salary;

            return;
        }

        public Worker(
            int id, 
            string firstName, 
            string lastName, 
            string patronymic,
            string position,
            decimal salary)
            : this(firstName, lastName, patronymic, position, salary)
        {
            Id = id;

            return;
        }

        public Worker(int id) : this (id, "", "", "", "", 0)
        {
            return;
        }

        public override string ToString()
        {
            return $"Сотрудник {LastName} {FirstName} {Patronymic}: {Position}; оклад {Salary}";
        }

        public void Deconstruct(
            out string firstName,
            out string lastName,
            out string patronymic,
            out string position,
            out decimal salary)
        {
            firstName = $"N'{FirstName}'";
            lastName = $"N'{LastName}'";
            patronymic = $"N'{Patronymic}'";
            position = $"N'{Position}'";
            salary = Salary; 

            return;
        }

        public virtual string Repr()
        {
            (string fName, string lName, string patronymic, string position, decimal salary) = this;
            return (fName, lName, patronymic, position, salary).ToString();
        }

        public virtual IEnumerable<object> Disassemble()
        {
            return new object[] { FirstName, LastName, Patronymic, Position, Salary };
        }
    }

    /// <summary>
    /// Класс сотрудника со ссылкой на подразделение.
    /// </summary>
    public class DepartmentWorker : Worker
    {
        protected int _departmentId;

        public virtual int DepartmentId
        {
            get => _departmentId;
            set => _departmentId = value;
        }

        public DepartmentWorker() : base() { return; }

        public DepartmentWorker(
            string firstName, 
            string lastName, 
            string patronymic,
            string position,
            decimal salary,
            int departmentId)
            : base(firstName, lastName, patronymic, position, salary)
        {
            DepartmentId = departmentId;

            return;
        }

        public DepartmentWorker(
            int id, 
            string firstName, 
            string lastName, 
            string patronymic,
            string position,
            decimal salary,
            int departmentId)
            : this(firstName, lastName, patronymic, position, salary, departmentId)
        {
            Id = id;

            return;
        }

        public DepartmentWorker(int id) : this(id, "", "", "", "", 0, 0)
        {
            return;
        }

        public void Deconstruct(
            out string firstName,
            out string lastName,
            out string patronymic,
            out string position,
            out decimal salary,
            out int departmentId)
        {
            this.Deconstruct(out firstName, out lastName, out patronymic, out position, out salary);
            departmentId = DepartmentId;

            return;
        }

        public override string Repr()
        {
            (string fName, string lName, string patronymic, string position, decimal salary, int departmentId)
                = this;
            return (fName, lName, patronymic, position, salary, departmentId).ToString();
        }

        public override IEnumerable<object> Disassemble()
        {
            return base.Disassemble().Concat(new object[] { DepartmentId });
        }
    }
}
