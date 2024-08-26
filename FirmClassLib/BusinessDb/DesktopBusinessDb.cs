using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using System.Data;
using System.Data.SqlClient;

namespace FirmClassLib.BusinessDb
{
    public class DesktopBusinessDb : BusinessDb<DepartmentWorker>
    {
        protected override string DbName { get => nameof(DesktopBusinessDb); }

        public override void InitWorkerType()
        {
            RegisterWorkerTypeInfo(
                new[] { nameof(DepartmentWorker.FirstName), nameof(DepartmentWorker.LastName), nameof(DepartmentWorker.Patronymic), nameof(DepartmentWorker.Position), nameof(DepartmentWorker.Salary), nameof(DepartmentWorker.DepartmentId) },
                new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Decimal, SqlDbType.Int });

            return;
        }

        protected override DepartmentWorker ReadWorker(SqlDataReader workersReader)
        {
            int id = workersReader.GetInt32(nameof(Worker.Id)),
                departmentId = workersReader.GetInt32(nameof(DepartmentWorker.DepartmentId));
            string firstName = workersReader.GetString(nameof(Worker.FirstName)),
                   lastName = workersReader.GetString(nameof(Worker.LastName)),
                   patronymic = workersReader.GetString(nameof(Worker.Patronymic)),
                   position = workersReader.GetString(nameof(Worker.Position));
            decimal salary = workersReader.GetDecimal(nameof(Worker.Salary));

            return new DepartmentWorker(id, firstName, lastName, patronymic, position, salary, departmentId);
        }
    }
}
