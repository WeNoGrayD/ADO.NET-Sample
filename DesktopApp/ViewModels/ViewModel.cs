using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        protected MainWindow _app;

        public ViewModel(MainWindow app)
        {
            _app = app;
            InitializeViewModel();

            return;
        }

        protected abstract void InitializeViewModel();

        public virtual void InitAfterViewInitialization()
        {
            PutOnEventHandlers();

            return;
        }

        public abstract Task LoadDataAsync();

        protected abstract void PutOnEventHandlers();

        protected void PerformActionInUIThread(Action toDo)
        {
            _app.Dispatcher.Invoke(toDo);

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
