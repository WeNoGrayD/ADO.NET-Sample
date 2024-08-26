using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopApp.Components;
using System.Collections.ObjectModel;
using FirmClassLib.BusinessDb;
using FirmClassLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp.ViewModels
{
    public class BusinessViewModel : ViewModel, IDisposable
    {
        DesktopBusinessDb _db;

        DepartmentsViewModel _departmentsViewModel;

        WorkersViewModel _workersViewModel;

        public BusinessViewModel(MainWindow app)
            : base(app)
        {
            return;
        }

        public void Dispose()
        {
            _db.Dispose();

            return;
        }

        protected override void InitializeViewModel()
        {
            _db = new DesktopBusinessDb();
            _departmentsViewModel = new DepartmentsViewModel(_app, _db);
            _workersViewModel = new WorkersViewModel(_app, _db, _departmentsViewModel);

            ChangeCulture();

            Task.Run(LoadDataAsync);

            return;
        }

        private void ChangeCulture()
        {
            CultureInfo culture = new CultureInfo("ru-RU");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage
                    .GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

            return;
        }

        public override void InitAfterViewInitialization()
        {
            base.InitAfterViewInitialization();
            _app.tiDepartments.DataContext = _departmentsViewModel;
            _app.tiWorkers.DataContext = _workersViewModel;
            _departmentsViewModel.InitAfterViewInitialization();
            _workersViewModel.InitAfterViewInitialization();

            return;
        }

        protected override void PutOnEventHandlers()
        {
            _app.Closed += (_, _) => this.Dispose();
            _app.tcTabs.SelectionChanged += UpdateTableOnTabSelectionChanged;

            return;
        }

        public override async Task LoadDataAsync()
        {
            await _departmentsViewModel.LoadDataAsync();
            await _workersViewModel.LoadDataAsync();

            return;
        }

        public async void UpdateTableOnTabSelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0) return;

            switch (e.RemovedItems[0])
            {
                case TabItem ti when ti.Equals(_app.tiDepartments):
                    {
                        _departmentsViewModel.UpdateDepartmentsOnLostFocusAsync(sender, e);

                        break;
                    }
                case TabItem ti when ti.Equals(_app.tiWorkers):
                    {
                        _workersViewModel.UpdateWorkersOnLostFocusAsync(sender, e);

                        break;
                    }
                default: return;
            }

            return;
        }
    }
}
