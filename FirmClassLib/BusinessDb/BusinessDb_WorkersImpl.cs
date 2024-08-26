using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using System.Data;
using System.Data.SqlClient;

namespace FirmClassLib.BusinessDb
{
    public abstract partial class BusinessDb<TWorker>
        : IDepartmentsContainer, IWorkersContainer<TWorker>
        where TWorker : Worker
    {
        public async Task AddWorkersAsync(IEnumerable<TWorker> samples)
        {
            await AddItemsAsync(samples, 5);

            return;
        }

        public async Task AddWorkersAsync(params TWorker[] samples)
        {
            await AddWorkersAsync((IEnumerable<TWorker>)samples);

            return;
        }

        public async Task<int> AddWorkerAndReturnIdAsync(
            TWorker record)
        {
            return await AddItemAndReturnIdAsync(record);
        }

        public async IAsyncEnumerable<TWorker> GetWorkersAsync()
        {
            using SqlConnection connection = await ConnectAsync();

            await foreach (TWorker worker in SelectAsync(connection, ReadWorker))
                yield return worker;
        }

        public async IAsyncEnumerable<TWorker> GetOrderedWorkersAsync()
        {
            using SqlConnection connection = await ConnectAsync();

            await foreach (TWorker worker 
                       in SelectOrderByAsync(
                           connection, 
                           ReadWorker,
                           new[] { nameof(Worker.LastName), nameof(Worker.FirstName), nameof(Worker.Patronymic)}))
                yield return worker;
        }

        protected abstract TWorker ReadWorker(SqlDataReader workersReader);

        public async Task<TWorker> SelectWorkerById(int id)
        {
            Worker pattern = new Worker(id);
            using SqlConnection connection = await ConnectAsync();
            SqlCommand selectCmd = BuildSelectWhereCommand<Worker, TWorker>(
                new (string, Func<Worker, object>)[]
                    { (nameof(Worker.Id), (Worker w) => w.Id) },
                pattern,
                connection);

            return await SelectSingleAsync(SelectAsync(
                selectCmd, ReadWorker),
                $"Сотрудников с идентификатором '{id}' в таблице не числится.",
                $"Сотрудников с идентификатором '{id}' в таблице больше одного.");
        }

        public async Task UpdateWorkerByIdAsync(
            IEnumerable<(string ColumnName, Func<TWorker, object> ValueFactory)> setters,
            TWorker updatePattern,
            Worker idPattern)
        {
            using SqlConnection connection = await ConnectAsync();
            SqlCommand updateCmd = BuildUpdateWhereCommand<Worker, TWorker>(
                setters,
                new (string, Func<Worker, object>)[]
                    { (nameof(Worker.Id), (Worker d) => d.Id) },
                updatePattern,
                idPattern,
                connection);

            await updateCmd.ExecuteNonQueryAsync();

            return;
        }

        public async Task UpdateWorkerByNameAsync(
            IEnumerable<(string ColumnName, Func<TWorker, object> ValueFactory)> setters,
            TWorker updatePattern,
            Worker firstNamePattern)
        {
            using SqlConnection connection = await ConnectAsync();
            SqlCommand updateCmd = BuildUpdateWhereCommand<Worker, TWorker>(
                setters,
                new (string, Func<Worker, object>)[]
                    { (nameof(Worker.FirstName), (Worker d) => d.FirstName) },
                updatePattern,
                firstNamePattern,
                connection);

            await updateCmd.ExecuteNonQueryAsync();

            return;
        }

        public async Task DelWorkersAsync(
            IEnumerable<TWorker> records)
        {
            await DelItemsAsync(records);

            return;
        }
    }
}
