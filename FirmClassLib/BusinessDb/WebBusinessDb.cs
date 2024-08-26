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
    public class WebBusinessDb : BusinessDb<Worker>
    {
        protected override string DbName { get => nameof(WebBusinessDb); }

        public override void InitWorkerType()
        {
            RegisterWorkerTypeInfo(
                new[] { nameof(Worker.FirstName), nameof(Worker.LastName), nameof(Worker.Patronymic), nameof(Worker.Position), nameof(Worker.Salary) },
                new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Decimal, });

            return;
        }

        protected override Worker ReadWorker(SqlDataReader workersReader)
        {
            int id = workersReader.GetInt32(nameof(Worker.Id));
            string firstName = workersReader.GetString(nameof(Worker.FirstName)),
                   lastName = workersReader.GetString(nameof(Worker.LastName)),
                   patronymic = workersReader.GetString(nameof(Worker.Patronymic)),
                   position = workersReader.GetString(nameof(Worker.Position));
            decimal salary = workersReader.GetDecimal(nameof(Worker.Salary));

            return new Worker(id, firstName, lastName, patronymic, position, salary);
        }
    }
}
