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

namespace DesktopApp.ViewModels
{
    public class WorkersViewModel : ViewModel
    {
        private FirmClassLib.BusinessDb.IWorkersContainer<
            FirmClassLib.DepartmentWorker> _db;

        private DepartmentsViewModel _departmentsViewModel;

        public ObservableCollection<DepartmentWorkerModel> Workers;

        public ObservableCollection<DepartmentWorkerModel> WorkerToAdd;

        private DepartmentWorkerModel _prevSelectedWorker;

        public WorkersViewModel(
            MainWindow app,
            FirmClassLib.BusinessDb.IWorkersContainer<
            FirmClassLib.DepartmentWorker> db,
            DepartmentsViewModel departmentsViewModel)
            : base(app)
        {
            _db = db;
            _departmentsViewModel = departmentsViewModel;

            return;
        }

        protected override void InitializeViewModel()
        {
            Workers = new ObservableCollection<DepartmentWorkerModel>();
            WorkerToAdd = new ObservableCollection<DepartmentWorkerModel>();
            WorkerToAdd.Add(DepartmentWorkerModel.Empty());

            return;
        }

        protected override void PutOnEventHandlers()
        {
            _app.btnDelWorker.Click += DeleteWorkersAsync;
            _app.btnAddWorker.Click += AddWorkerAsync;
            _app.dgWorkers.SelectedCellsChanged += UpdateWorkersOnUnselectAsync;
            _app.Closed += UpdateWorkersOnLostFocusAsync;
            //_app.dgWorkers.LostFocus += UpdateWorkerAsync;

            return;
        }

        private async Task LoadWorkers()
        {
            await foreach (var record in _db.GetOrderedWorkersAsync())
            {
                Workers.Add(new DepartmentWorkerModel(record));
            }

            PerformActionInUIThread(() =>
            {
                _app.dgWorkers.ItemsSource = Workers;
                _app.dgWorkerToAdd.ItemsSource = WorkerToAdd;
            });

            return;
        }

        public override async Task LoadDataAsync()
        {
            await LoadWorkers();

            return;
        }

        public async void AddWorkerAsync(object sender, RoutedEventArgs e)
        {
            DepartmentWorkerModel workerToAdd = WorkerToAdd.First();

            if (!VerifyWorker(workerToAdd)) return;

            int recordId = await _db.AddWorkerAndReturnIdAsync(workerToAdd);
            DepartmentWorkerModel newWorker =
                new DepartmentWorkerModel(await _db.SelectWorkerById(recordId));
            Workers.Add(newWorker);
            // Инициализация хоть каким-нибудь значением.
            if (_prevSelectedWorker is null) _prevSelectedWorker = newWorker;

            workerToAdd.Clear();

            return;
        }

        private bool VerifyWorker(DepartmentWorkerModel worker)
        {
            bool verificationResult = true;

            if (!(verificationResult = worker.Verify()))
            {
                MessageBox.Show(
                    "Сотрудник должен быть назначен в подразделение, " +
                    "его имя, фамилия, отчество и должность не должны состоять " +
                    "только из пробелов или быть пустыми.", 
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return verificationResult;
        }

        public async void UpdateWorkersOnUnselectAsync(
            object sender, 
            SelectedCellsChangedEventArgs e)
        {
            foreach (DepartmentWorkerModel updated
                 in e.RemovedCells.Select(c => c.Item)
                                  .ToArray()
                                  .OfType<DepartmentWorkerModel>())
            {
                await UpdateWorkerAsync(updated);
            }

            return;
        }

        public async void UpdateWorkersOnLostFocusAsync(
            object sender,
            EventArgs e)
        {
            foreach (DepartmentWorkerModel updated
                 in _app.dgWorkers.SelectedItems
                                .Cast<DepartmentWorkerModel>()
                                .ToArray())
            {
                await UpdateWorkerAsync(updated);
            }

            return;
        }

        public async Task UpdateWorkerAsync(DepartmentWorkerModel updated)
        {
            await updated.CommitChangesAsync(_db);
            DepartmentModel dep =
                _departmentsViewModel
                .FindDepartmentById(updated.DepartmentId);
            await dep
                .CommitChangesAsync(_db as IDepartmentsContainer);

            return;
        }

        public async void DeleteWorkersAsync(
            object sender,
            EventArgs e)
        {
            DepartmentWorkerModel[] deleted = _app.dgWorkers.SelectedItems
                                .Cast<DepartmentWorkerModel>()
                                .ToArray();

            await _db.DelWorkersAsync(deleted);
            for (int i = 0; i < deleted.Length; i++)
                Workers.Remove(deleted[i]);

            return;
        }
    }
}
