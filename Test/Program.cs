using FirmClassLib;
using FirmClassLib.BusinessDb;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello, World!");
            //Console.WriteLine((1, "rope", DateTime.Now).ToString());

            Task.Run(TestDesktopDbAsync).ConfigureAwait(false);

            Console.ReadKey();
        }

        private static async Task TestDesktopDbAsync()
        {
            DesktopBusinessDb db = new DesktopBusinessDb();

            /*
            await db.AddDepartmentsAsync(
                new Department("Отдел разработки"),
                new Department("Бухгалтерия"),
                new Department("Юридический отдел"));
            */

            //await db.AddDepartmentsAsync(
            //   new Department("Отдел информационной безопасности"));

            //await PrintDepartments(db);

            /*
            Department d1 = (await db.SelectDepartmentByNameAsync("Отдел разработки")),
                       d2 = (await db.SelectDepartmentByNameAsync("Бухгалтерия")),
                       d3 = (await db.SelectDepartmentByNameAsync("Юридический отдел"));
            DepartmentWorker w1 = new DepartmentWorker("Иван", "Иванов", "Иванович", "Старший программист", 360000, d1.Id),
                w2 = new DepartmentWorker("Пётр", "Петров", "Петрович", "Главный бухгалтер", 240000, d2.Id),
                w3 = new DepartmentWorker("Сидор", "Сидоров", "Сидорович", "Юрисконсульт", 120000, d3.Id);
            */

            //await db.AddWorkersAsync(w1, w2, w3);

            await UpdateDepartmentWorkersTest(db);

            await PrintWorkers(db);
        }

        private static async Task PrintWorkers<TWorker>(
            IWorkersContainer<TWorker> db)
            where TWorker : Worker
        {
            await foreach (Worker w in db.GetWorkersAsync())
                Console.WriteLine(w.ToString());

            return;
        }

        private static async Task PrintDepartments(IDepartmentsContainer db)
        {
            await foreach (Department d in db.GetDepartmentsAsync())
                Console.WriteLine(d.ToString());

            return;
        }

        private static async Task UpdateDepartmentWorkersTest(
            IWorkersContainer<DepartmentWorker> db)
        {
            Worker namePattern = new Worker() { FirstName = "Сидор" };
            DepartmentWorker updatePattern = await db.SelectWorkerById(3);
            updatePattern.FirstName = "Исильдур";
            updatePattern.Salary = 45_000M;
            updatePattern.DepartmentId = 1;

            await db.UpdateWorkerByNameAsync(
                new (string, Func<DepartmentWorker, object>)[]
                { 
                    (nameof(Worker.FirstName), (dw) => dw.FirstName),
                    (nameof(Worker.Salary), (dw) => dw.Salary),
                    (nameof(DepartmentWorker.DepartmentId), (dw) => dw.DepartmentId),
                },
                updatePattern,
                namePattern);

            return;
        }
    }
}
