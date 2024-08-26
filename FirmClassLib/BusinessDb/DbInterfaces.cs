using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmClassLib.BusinessDb
{
    public interface IDepartmentsContainer
    {
        Task AddDepartmentsAsync(IEnumerable<Department> samples);

        Task AddDepartmentsAsync(params Department[] samples);

        Task<int> AddDepartmentAndReturnIdAsync(Department record);

        IAsyncEnumerable<Department> GetDepartmentsAsync();

        IAsyncEnumerable<Department> GetOrderedDepartmentsAsync();

        Task<Department> SelectDepartmentByIdAsync(int id);

        Task UpdateDepartmentByIdAsync(
            Department updatePattern,
            Department idPattern);

        Task DelDepartmentsAsync(IEnumerable<Department> records);
    }

    public interface IWorkersContainer<TWorker>
        where TWorker : Worker
    {
        Task AddWorkersAsync(IEnumerable<TWorker> samples);

        Task AddWorkersAsync(params TWorker[] samples);

        Task<int> AddWorkerAndReturnIdAsync(
            TWorker record);

        IAsyncEnumerable<TWorker> GetWorkersAsync();

        IAsyncEnumerable<TWorker> GetOrderedWorkersAsync();

        Task<TWorker> SelectWorkerById(int id);

        Task UpdateWorkerByIdAsync(
            IEnumerable<(string ColumnName, Func<TWorker, object> ValueFactory)> setters,
            TWorker updatePattern,
            Worker idPattern);

        Task UpdateWorkerByNameAsync(
            IEnumerable<(string ColumnName, Func<TWorker, object> ValueFactory)> setters,
            TWorker updatePattern,
            Worker firstNamePattern);

        Task DelWorkersAsync(IEnumerable<TWorker> records);
    }
}
