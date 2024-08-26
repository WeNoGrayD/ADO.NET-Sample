using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirmClassLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FirmClassLib.BusinessDb;
using System.Runtime.Intrinsics.Arm;
using System.Windows;

namespace DesktopApp.Models
{
    public class DepartmentModel
        : Department, INotifyPropertyChanged
    {
        private Department _innerState;

        private bool _hasChanged = false;

        public bool HasChanged { get => _hasChanged; }

        public override string Name
        {
            get => base.Name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public const string DefaultName = "Без названия";

        /*
        private static (string Column, Func<Department, object> ValueFactory)[]
            _propertyGetters = new (string, Func<Department, object>)[]
            {
                ( nameof(Name), (d) => d.Name )
            };
        */

        public DepartmentModel(Department dep)
        {
            InitWith(dep);
            PropertyChanged += (_, _) => _hasChanged = true;

            return;
        }

        private void InitWith(Department dep)
        {
            Id = dep.Id;
            Name = dep.Name;

            _hasChanged = false;
            _innerState = dep;

            return;
        }

        public static DepartmentModel Null()
        {
            return NullDepartmentModel.Instance;
        }

        public static DepartmentModel Empty()
        {
            return new DepartmentModel(EmptyInner());
        }

        private static Department EmptyInner()
        {
            return new Department(-1, DefaultName);
        }

        public void Clear()
        {
            InitWith(EmptyInner());

            return;
        }

        public bool Verify()
        {
            bool verificationResult = true;

            this.ToGroom();

            if (string.IsNullOrEmpty(this.Name) ||
                this.Name == DepartmentModel.DefaultName)
            {
                verificationResult = false;
            }

            return verificationResult;
        }

        public async Task CommitChangesAsync(IDepartmentsContainer db)
        {
            if (!_hasChanged) return;

            this.ToGroom();

            await db.UpdateDepartmentByIdAsync(
                this,
                _innerState);
            _hasChanged = false;

            return;
        }

        private void ToGroom()
        {
            this._name = _name.Trim();

            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));

            return;
        }

        private class NullDepartmentModel
            : DepartmentModel
        {
            public static DepartmentModel Instance { get; }
                = new NullDepartmentModel();

            public override string Name
            {
                get => base.Name;
                // Название нельзя поменять
                set
                {
                    return;
                }
            }

            private NullDepartmentModel()
                : base(new Department(-1, "Не назначен"))
            {
                _name = _innerState.Name;
                //OnPropertyChanged(nameof(Name));

                return;
            }
        }
    }
}
