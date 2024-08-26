using DesktopApp.Components;
using FirmClassLib;
using FirmClassLib.BusinessDb;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DesktopApp.Models;
using System.Windows.Automation;
using System.Windows.Data;

namespace DesktopApp.ViewModels
{
    public class DepartmentsViewModel : ViewModel
    {
        private FirmClassLib.BusinessDb.IDepartmentsContainer _db;

        public ObservableCollection<DepartmentModel> Departments;

        public ObservableCollection<DepartmentModel> DepartmentToAdd;

        private IDictionary<int, int> _departmentsSearchTable;

        private DepartmentModel _prevSelectedDepartment;

        public DepartmentsViewModel(
            MainWindow app,
            FirmClassLib.BusinessDb.IDepartmentsContainer db)
            : base(app)
        {
            _db = db;

            return;
        }

        protected override void InitializeViewModel()
        {
            _departmentsSearchTable = new Dictionary<int, int>();
            Departments = new ObservableCollection<DepartmentModel>();
            DepartmentToAdd = new ObservableCollection<DepartmentModel>();
            DepartmentToAdd.Add(DepartmentModel.Empty());

            ResourceDictionary standardComponents = new ResourceDictionary();
            DepartmentIdConverter depIdConverter =
                new DepartmentIdConverter(this);
            var decimalConverter = new Components.DecimalConverter();

            standardComponents.Add(
                nameof(DepartmentIdConverter),
                depIdConverter);
            standardComponents.Add(
                nameof(Components.DecimalConverter),
                decimalConverter);
            standardComponents.Add("Departments", Departments);

            _app.Resources.MergedDictionaries.Add(standardComponents);

            return;
        }

        protected override void PutOnEventHandlers()
        {
            _app.btnDelDepartment.Click += DeleteDepartmentsAsync;
            _app.btnAddDepartments.Click += AddDepartmentAsync;
            //_app.dgDepartments.LostFocus += UpdateDepartmentAsync;
            _app.dgDepartments.SelectedCellsChanged += UpdateDepartmentsOnUnselectAsync;
            _app.Closed += UpdateDepartmentsOnLostFocusAsync;

            return;
        }

        public override async Task LoadDataAsync()
        {
            await LoadDepartments();

            return;
        }

        private async Task LoadDepartments()
        {
            int i = 0;
            DepartmentModel dep;

            Departments.Add(DepartmentModel.Null());
            await foreach (var record in _db.GetOrderedDepartmentsAsync())
            {
                dep = new DepartmentModel(record);
                Departments.Add(dep);
                _departmentsSearchTable.Add(dep.Id, ++i);
            }

            PerformActionInUIThread(() =>
            {
                _app.dgDepartments.ItemsSource = Departments;
                _app.dgDepartmentToAdd.ItemsSource = DepartmentToAdd;
            });

            return;
        }

        public DepartmentModel FindDepartmentById(int depId)
        {
            if (_departmentsSearchTable.ContainsKey(depId))
            {
                int depIndex = _departmentsSearchTable[depId];

                return Departments.ElementAt(depIndex);
            }

            return DepartmentModel.Null();
        }

        public async void AddDepartmentAsync(object sender, RoutedEventArgs e)
        {
            DepartmentModel depToAdd = DepartmentToAdd.First();

            if (!VerifyDepartment(depToAdd)) return;

            int recordId = await _db.AddDepartmentAndReturnIdAsync(depToAdd);
            DepartmentModel newDep =
                new DepartmentModel(await _db.SelectDepartmentByIdAsync(recordId));
            _departmentsSearchTable.Add(recordId, Departments.Count);
            Departments.Add(newDep);
            // Инициализация хоть каким-нибудь значением.
            if (_prevSelectedDepartment is null) _prevSelectedDepartment = newDep;

            depToAdd.Clear();

            return;
        }

        private bool VerifyDepartment(DepartmentModel dep)
        {
            bool verificationResult = true;

            if (!(verificationResult = dep.Verify()))
            {
                MessageBox.Show(
                    "Подразделение должно иметь название, которое не должно " +
                    "быть пустым или состоять только из пробелов.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return verificationResult;
        }

        public async void UpdateDepartmentsOnUnselectAsync(
            object sender,
            SelectedCellsChangedEventArgs e)
        {
            foreach (DepartmentModel updated
                 in e.RemovedCells.Select(c => c.Item)
                                  .ToArray()
                                  .OfType<DepartmentModel>())
            {
                await UpdateDepartmentAsync(updated);
            }

            return;
        }

        public async void UpdateDepartmentsOnLostFocusAsync(
            object sender,
            EventArgs e)
        {
            foreach (DepartmentModel updated
                 in _app.dgDepartments.SelectedItems
                                .Cast<DepartmentModel>()
                                .ToArray())
            {
                await UpdateDepartmentAsync(updated);
            }

            return;
        }

        public async Task UpdateDepartmentAsync(DepartmentModel updated)
        {
            await updated.CommitChangesAsync(_db);

            return;
        }

        public async void DeleteDepartmentsAsync(
            object sender,
            EventArgs e)
        {
            List<Department> deleted = _app.dgDepartments.SelectedItems
                                .Cast<Department>()
                                .ToList();

            if (deleted.Count == 0) return;

            await _db.DelDepartmentsAsync(deleted);

            int mod = 1, curDepId = -1;

            if (deleted.Count == 1)
            {
                ModifySearchTable(0);

                return;
            }

            deleted.Sort((d1, d2) => d1.Id.CompareTo(d2.Id));

            int nextDepId = -1, i;
            for (i = 0; i < deleted.Count - 1; i++)
            {
                ModifySearchTableWithinLimits(i);
            }
            ModifySearchTable(i);

            return;

            void ModifySearchTable(int i)
            {
                curDepId = deleted[i].Id;

                if (curDepId == -1) return;

                Departments.RemoveAt(_departmentsSearchTable[curDepId]);
                _departmentsSearchTable.Remove(curDepId);

                foreach (int depId in _departmentsSearchTable.Keys)
                {
                    if (depId < curDepId) continue;

                    _departmentsSearchTable[depId] -= mod;
                }

                return;
            }

            void ModifySearchTableWithinLimits(int i)
            {
                curDepId = deleted[i].Id;

                if (curDepId == -1) return;

                nextDepId = deleted[i + 1].Id;

                Departments.RemoveAt(_departmentsSearchTable[curDepId]);
                _departmentsSearchTable.Remove(curDepId);

                foreach (int depId in _departmentsSearchTable.Keys)
                {
                    if (depId < curDepId || depId > nextDepId) continue;

                    _departmentsSearchTable[depId] -= mod;
                }

                mod++;

                return;
            }
        }
    }
}
