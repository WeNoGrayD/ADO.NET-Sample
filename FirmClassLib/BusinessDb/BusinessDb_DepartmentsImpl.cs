using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace FirmClassLib.BusinessDb
{
    public abstract partial class BusinessDb<TWorker>
        : IDepartmentsContainer, IWorkersContainer<TWorker>
        where TWorker : Worker
    {
        public async Task AddDepartmentsAsync(IEnumerable<Department> samples)
        {
            await AddItemsAsync(samples, 10);

            return;
        }

        public async Task AddDepartmentsAsync(params Department[] samples)
        {
            await AddDepartmentsAsync((IEnumerable<Department>)samples);

            return;
        }

        public async Task<int> AddDepartmentAndReturnIdAsync(Department record)
        {
            return await AddItemAndReturnIdAsync(record);
        }

        public async IAsyncEnumerable<Department> GetDepartmentsAsync()
        {
            using SqlConnection connection = await ConnectAsync();

            await foreach (Department department in SelectAsync(connection, ReadDepartment))
                yield return department;
        }

        public async IAsyncEnumerable<Department> GetOrderedDepartmentsAsync()
        {
            using SqlConnection connection = await ConnectAsync();

            await foreach (Department department 
                        in SelectOrderByAsync(
                            connection, 
                            ReadDepartment,
                            new[] { nameof(Department.Name) }))
                yield return department;
        }

        private Department ReadDepartment(SqlDataReader departmentsReader)
        {
            int id = departmentsReader.GetInt32(nameof(Department.Id));
            string name = departmentsReader.GetString(nameof(Department.Name));

            return new Department(id, name);
        }

        public async Task<Department> SelectDepartmentByNameAsync(string name)
        {
            Department pattern = new Department() { Name = name };
            using SqlConnection connection = await ConnectAsync();
            SqlCommand selectCmd = BuildSelectWhereCommand<Department>(
                new (string, Func<Department, object>)[] 
                    { (nameof(Department.Name), (Department d) => d.Name) },
                pattern,
                connection);

            return await SelectSingleAsync(SelectAsync(
                selectCmd, ReadDepartment),
                $"Подразделений с именем '{name}' в таблице не числится.",
                $"Подразделений с именем '{name}' в таблице больше одного.");
        }

        public async Task<Department> SelectDepartmentByIdAsync(int id)
        {
            Department pattern = new Department(id);
            using SqlConnection connection = await ConnectAsync();
            SqlCommand selectCmd = BuildSelectWhereCommand<Department>(
                new (string, Func<Department, object>)[]
                    { (nameof(Department.Id), (Department d) => d.Id) },
                pattern,
                connection);

            return await SelectSingleAsync(SelectAsync(
                selectCmd, ReadDepartment),
                $"Подразделений с идентификатором '{id}' в таблице не числится.",
                $"Подразделений с идентификатором '{id}' в таблице больше одного.");
        }

        public async Task UpdateDepartmentByIdAsync(
            Department updatePattern,
            Department idPattern)
        {
            using SqlConnection connection = await ConnectAsync();
            SqlCommand updateCmd = BuildUpdateWhereCommand<Department>(
                new (string, Func<Department, object>)[]
                    { (nameof(Department.Name), (Department d) => d.Name) },
                new (string, Func<Department, object>)[]
                    { (nameof(Department.Id), (Department d) => d.Id) },
                updatePattern,
                idPattern,
                connection);

            await updateCmd.ExecuteNonQueryAsync();

            return;
        }

        public async Task DelDepartmentsAsync(
            IEnumerable<Department> records)
        {
            await DelItemsAsync(records);

            return;
        }
    }
}
