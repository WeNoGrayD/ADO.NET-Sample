using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirmClassLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FirmClassLib.BusinessDb;
using System.Windows;

namespace DesktopApp.Models
{
    public class DepartmentWorkerModel
        : DepartmentWorker, INotifyPropertyChanged
    {
        public DepartmentWorker _innerState;

        private bool _hasChanged = false;

        public override string FirstName
        {
            get => base.FirstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string LastName
        {
            get => base.LastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string Patronymic
        {
            get => base.Patronymic;
            set
            {
                if (_patronymic != value)
                {
                    _patronymic = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string Position
        {
            get => base.Position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }

        public override decimal Salary
        {
            get => base.Salary;
            set
            {
                if (_salary != value)
                {
                    _salary = value;
                    OnPropertyChanged();
                }
            }
        }

        public override int DepartmentId
        {
            get => base.DepartmentId;
            set
            {
                if (_departmentId != value)
                {
                    _departmentId = value;
                    OnPropertyChanged();
                }
            }
        }

        private static (string Column, Func<DepartmentWorker, object> ValueFactory)[]
            _propertyGetters = new (string, Func<DepartmentWorker, object>)[]
            {
                ( nameof(FirstName), (dw) => dw.FirstName ),
                ( nameof(LastName), (dw) => dw.LastName ),
                ( nameof(Patronymic), (dw) => dw.Patronymic ),
                ( nameof(Position), (dw) => dw.Position ),
                ( nameof(Salary), (dw) => dw.Salary ),
                ( nameof(DepartmentId), (dw) => dw.DepartmentId )
            };

        public DepartmentWorkerModel(DepartmentWorker depWorker)
        {
            InitWith(depWorker);
            PropertyChanged += (_, _) => _hasChanged = true;

            return;
        }

        private void InitWith(DepartmentWorker depWorker)
        {
            Id = depWorker.Id;
            FirstName = depWorker.FirstName;
            LastName = depWorker.LastName;
            Patronymic = depWorker.Patronymic;
            Position = depWorker.Position;
            Salary = depWorker.Salary;
            DepartmentId = depWorker.DepartmentId;

            _innerState = depWorker;
            _hasChanged = false;

            return;
        }

        public static DepartmentWorkerModel Empty()
        {
            return new DepartmentWorkerModel(EmptyInner());
        }

        private static DepartmentWorker EmptyInner()
        {
            return new DepartmentWorker() { DepartmentId = -1 };
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

            if (this.DepartmentId < 0 ||
                string.IsNullOrEmpty(this.FirstName) ||
                string.IsNullOrEmpty(this.LastName) ||
                string.IsNullOrEmpty(this.Patronymic) ||
                string.IsNullOrEmpty(this.Position))
            {
                verificationResult = false;
            }

            return verificationResult;
        }

        public async Task CommitChangesAsync(IWorkersContainer<DepartmentWorker> db)
        {
            if (!_hasChanged) return;

            this.ToGroom();

            await db.UpdateWorkerByIdAsync(
                _propertyGetters,
                this,
                _innerState);
            _hasChanged = false;

            return;
        }

        private void ToGroom()
        {
            this._firstName = _firstName.Trim();
            this._lastName = _lastName.Trim();
            this._patronymic = _patronymic.Trim();
            this._position = _position.Trim();

            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));

            return;
        }
    }
}
