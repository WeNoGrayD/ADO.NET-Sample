using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmClassLib
{
    public class WorkerInDepartment
    {
        public int WorkerId { get; set; }

        public int DepartmentId { get; set; }

        public DateOnly FirstDateOfWork { get; set; }

        public DateOnly LastDateOfWork { get; set; }

        public WorkerInDepartment(
            int workerId, 
            int departmentId,
            DateOnly firstDate,
            DateOnly lastDate = default(DateOnly))
        {
            WorkerId = workerId;
            DepartmentId = departmentId;
            FirstDateOfWork = firstDate;
            
            if (lastDate == default(DateOnly))
                lastDate = DateOnly.FromDateTime(DateTime.Now);
            LastDateOfWork = lastDate;

            return;
        }

        public WorkerInDepartment(
            Worker worker,
            Department department,
            DateOnly firstDate,
            DateOnly lastDate = default(DateOnly))
            : this(worker.Id, department.Id, firstDate, lastDate)
        {
            return;
        }
    }
}
