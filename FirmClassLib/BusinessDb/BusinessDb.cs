using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace FirmClassLib.BusinessDb
{
    public abstract partial class BusinessDb<TWorker> 
        : IDepartmentsContainer, IWorkersContainer<TWorker>, IDisposable
        where TWorker : Worker
    {
        protected abstract string DbName { get; }

        private string _connectionStr;

        private SqlConnection _connection;

        private IDictionary<Type, TableAPI> _tablesByType =
            new Dictionary<Type, TableAPI>()
            {
                {
                    typeof(Department),
                    new TableAPI(
                        "Departments",
                        new string[] { nameof(Department.Name) },
                        new SqlDbType[] { SqlDbType.NVarChar })
                }
            };

        public BusinessDb()
        {
            _connectionStr = $@"Server=(localdb)\mssqllocaldb;Database={DbName};Trusted_Connection=True;";
            InitWorkerType();

            return;
        }

        public void Dispose()
        {
            if (_connection is not null)
                _connection.Dispose();
        }

        protected void RegisterTable(
            Type tableType,
            string tableName,
            string[] columns,
            SqlDbType[] colTypes)
        {
            _tablesByType.Add(tableType, new TableAPI(tableName, columns, colTypes));
        }

        protected void RegisterWorkerTypeInfo(
            string[] columns,
            SqlDbType[] colTypes)
        {
            RegisterTable(typeof(TWorker), "Workers", columns, colTypes);
        }

        public abstract void InitWorkerType();

        public async Task<SqlConnection> ConnectAsync()
        {
            _connection = new SqlConnection(_connectionStr);
            await _connection.OpenAsync();

            return _connection;
        }

        protected SqlCommand BuildInsertCommand<TData>(
            IEnumerable<TData> samples,
            SqlConnection connection)
            where TData : IDisassembleable
        {
            TableAPI table = _tablesByType[typeof(TData)];
            table.StartSession();

            IEnumerable<SqlParameter[]> cmdParams = 
                table.ExtractParameters(samples);

            SqlCommand cmd = new SqlCommand() { Connection = connection };
            StringBuilder insertCmdTextSb = new StringBuilder(
                $"INSERT INTO {table.Name} {table.GetColumnsRepr()} VALUES ");
            foreach (var recordParams in cmdParams)
            {
                insertCmdTextSb.Append( 
                    "(" + 
                    string.Join(',', recordParams.Select(rp => rp.ParameterName)) 
                    + "),");
                cmd.Parameters.AddRange(recordParams);
            }
            cmd.CommandText = insertCmdTextSb.ToString()[..^1];

            table.ResetSession();

            return cmd;
        }

        protected SqlCommand BuildInsertAndReturnIdCommand<TData>(
            TData record,
            SqlConnection connection)
            where TData : IDisassembleable
        {
            TableAPI table = _tablesByType[typeof(TData)];
            table.StartSession();

            SqlCommand cmd = new SqlCommand() { Connection = connection };
            string cmdBaseText = $"INSERT INTO {table.Name} {table.GetColumnsRepr()} VALUES ",
                   insertCmdText;
            SqlParameter[] cmdParams;
            SqlParameter idParam;

            idParam = table.ExtractIdOutputParameter(record, (r) => r.Id);
            cmdParams = table.ExtractParameters(record).ToArray();

            insertCmdText = cmdBaseText +
                $"({string.Join(',', cmdParams.Select(rp => rp.ParameterName))});" +
                $"SET {idParam.ParameterName}=SCOPE_IDENTITY()";

            cmd.CommandText = insertCmdText;
            cmd.Parameters.Add(idParam);
            cmd.Parameters.AddRange(cmdParams);

            table.ResetSession();

            return cmd;
        }

        protected delegate Task<T> SqlCmdExecAsync<T>(SqlCommand cmd);

        protected delegate SqlCommand SqlCmdFactory<TData>(
            IEnumerable<TData> samples,
            SqlConnection connection);

        protected async Task ExecuteCommandInBatchesAsync<TData, T>(
            IEnumerable<TData> samples,
            int batchSize,
            SqlConnection connection,
            SqlCmdFactory<TData> cmdFactory,
            SqlCmdExecAsync<T> cmdExecution)
            where TData : IDisassembleable
        {
            TData[] batch;
            SqlCommand cmd;

            while ((batch = samples.Take(batchSize).ToArray()).Length > 0)
            {
                cmd = cmdFactory(batch, connection);
                await cmdExecution(cmd);
                samples = samples.Skip(batchSize);
            }

            return;
        }

        protected async Task<int> SqlCmdExecNonQueryAsync(SqlCommand cmd)
        {
            return await cmd.ExecuteNonQueryAsync();
        }

        protected async Task AddItemsAsync<TData>(
            IEnumerable<TData> samples,
            int batchSize = 1)
            where TData : IDisassembleable
        {
            using SqlConnection connection = await ConnectAsync();

            await ExecuteCommandInBatchesAsync(
                samples,
                batchSize,
                connection,
                BuildInsertCommand<TData>,
                SqlCmdExecNonQueryAsync);

            _tablesByType[typeof(TData)].ResetSession();

            return;
        }

        protected async Task<int> AddItemAndReturnIdAsync<TData>(
            TData record)
            where TData : IDisassembleable
        {
            using SqlConnection connection = await ConnectAsync();

            SqlCommand insertCmd = BuildInsertAndReturnIdCommand(record, connection);
            await insertCmd.ExecuteNonQueryAsync();

            _tablesByType[typeof(TData)].ResetSession();

            return (int)insertCmd.Parameters[0].Value;
        }

        protected async Task DelItemsAsync<TData>(
            IEnumerable<TData> samples,
            int batchSize = 1)
            where TData : IDisassembleable
        {
            using SqlConnection connection = await ConnectAsync();
            SqlCommand delCmd;

            foreach (var record in samples)
            {
                delCmd = BuildDeleteWhereCommand(
                    new (string, Func<TData, object>)[] { ("Id", (d) => d.Id) },
                    record,
                    connection);
                await delCmd.ExecuteNonQueryAsync();
            }

            return;
        }

        protected async IAsyncEnumerable<TData> SelectAsync<TData>(
            SqlCommand cmd,
            Func<SqlDataReader, TData> deserializer)
        {
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) yield break;
            else
            {
                while (await reader.ReadAsync())
                {
                    yield return deserializer(reader);
                }
            }

            yield break;
        }

        protected IAsyncEnumerable<TData> SelectAsync<TData>(
            SqlConnection connection,
            Func<SqlDataReader, TData> deserializer)
        {
            SqlCommand readCmd = new SqlCommand(
                SelectQueryString(
                    new Dictionary<Type, IEnumerable<string>>
                        { { typeof(TData), null! } }), 
                connection);

            return SelectAsync<TData>(readCmd, deserializer);
        }

        protected IAsyncEnumerable<TData> SelectOrderByAsync<TData>(
            SqlConnection connection,
            Func<SqlDataReader, TData> deserializer,
            IEnumerable<string> orderingConditions)
            where TData : IDisassembleable
        {
            SqlCommand readCmd = new SqlCommand(
                SelectQueryString(
                    new Dictionary<Type, IEnumerable<string>>
                        { { typeof(TData), null! } }) +
                OrderByQueryString<TData>(orderingConditions),
                connection);

            return SelectAsync<TData>(readCmd, deserializer);
        }

        protected async Task<TData> SelectSingleAsync<TData>(
            IAsyncEnumerable<TData> query,
            string zeroEntriesErrMessage = "",
            string nonUnicEntryErrMessage = "")
        {
            TData dataRes = default!;
            byte error = 0b0;

            await foreach (TData data in query)
            {
                if ((error & 0b1) != 0)
                    throw new Exception(nonUnicEntryErrMessage);
                dataRes = data;
                error |= 0b11;
            }
            if ((error & 0b10) == 0)
                throw new Exception(zeroEntriesErrMessage);

            return dataRes;
        }

        protected SqlCommand BuildSelectWhereCommand<TData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData record,
            SqlConnection connection)
            where TData : IDisassembleable
        {
            return BuildSelectWhereCommand<TData, TData>(conditions, record, connection);
        }

        /// <summary>
        /// Простой запрос SELECT выбирает название таблицы в поле FROM
        /// по типу CTData, но условия для запроса WHERE используют
        /// базовый тип TData, что позволяет:
        /// 1) задавать имя таблицы в адаптере БД для CTData, а не TData;
        /// 2) в качестве записи-паттерна с заданными значениями для поиска
        /// передавать экземпляр типа TData, если CTData является параметром
        /// типа адаптера БД (данного класса, BusinessDb), в качестве которого
        /// выступает в данном случае иерархия классов Worker.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="CTData"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="record"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        protected SqlCommand BuildSelectWhereCommand<TData, CTData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData record,
            SqlConnection connection)
            where TData : IDisassembleable
            where CTData : TData
        {
            Action<SqlCommand> whereCallback;
            SqlCommand cmd = new SqlCommand(
                SelectQueryString(
                    new Dictionary<Type, IEnumerable<string>>()
                        { {typeof(CTData), null! } }) +
                WhereQueryString<TData, CTData>(conditions, record, out whereCallback),
                connection);
            whereCallback(cmd);

            _tablesByType[typeof(CTData)].ResetSession();

            return cmd;
        }

        protected SqlCommand BuildUpdateWhereCommand<TData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> setters,
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData updatePattern,
            TData wherePattern,
            SqlConnection connection)
            where TData : IDisassembleable
        {
            return BuildUpdateWhereCommand<TData, TData>(
                setters,
                conditions,
                updatePattern,
                wherePattern,
                connection);
        }

        protected SqlCommand BuildUpdateWhereCommand<TData, CTData>(
            IEnumerable<(string ColumnName, Func<CTData, object> ValueFactory)> setters,
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            CTData updatePattern,
            TData wherePattern,
            SqlConnection connection)
            where TData : IDisassembleable
            where CTData : TData
        {
            Action<SqlCommand> updateCallback, whereCallback;
            SqlCommand cmd = new SqlCommand(
                UpdateQueryString(setters, updatePattern, out updateCallback) +
                WhereQueryString<TData, CTData>(conditions, wherePattern, out whereCallback),
                connection);
            updateCallback(cmd);
            whereCallback(cmd);

            _tablesByType[typeof(CTData)].ResetSession();

            return cmd;
        }

        protected SqlCommand BuildDeleteWhereCommand<TData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData wherePattern,
            SqlConnection connection)
            where TData : IDisassembleable
        {
            return BuildDeleteWhereCommand<TData, TData>(
                conditions,
                wherePattern,
                connection);
        }

        protected SqlCommand BuildDeleteWhereCommand<TData, CTData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData wherePattern,
            SqlConnection connection)
            where TData : IDisassembleable
            where CTData : TData
        {
            Action<SqlCommand> whereCallback;
            SqlCommand cmd = new SqlCommand(
                DeleteQueryString<CTData>() +
                WhereQueryString<TData, CTData>(conditions, wherePattern, out whereCallback),
                connection);
            whereCallback(cmd);

            _tablesByType[typeof(CTData)].ResetSession();

            return cmd;
        }

        protected string SelectQueryString(
            IDictionary<Type, IEnumerable<string>> selectorsFromTables
            )
        {
            string baseSelector = "*";
            string selectorsRepr = baseSelector, fromTablesRepr = "";

            if (selectorsFromTables.Count == 1)
            {
                IEnumerable<string> selector = selectorsFromTables.Values.First();
                if (selector is not null)
                    selectorsRepr = string.Join(',', selector);
                fromTablesRepr = _tablesByType[selectorsFromTables.Keys.First()].Name;
            }
            else
            {
                TableAPI table;
                StringBuilder selectorsReprSb = new StringBuilder(""),
                              fromTablesReprSb = new StringBuilder("");
                IEnumerable<string> selector;

                foreach (Type tableType in selectorsFromTables.Keys)
                {
                    table = _tablesByType[tableType];
                    selector = selectorsFromTables[tableType];
                    selectorsReprSb.Append(
                        $"{string.Join(',', selector.Select(s => $"{table.Name}.{s}"))},");
                    fromTablesReprSb.Append($"{table.Name},");
                }
                selectorsRepr = selectorsReprSb.ToString()[..^1];
                fromTablesRepr = fromTablesRepr.ToString()[..^1];
            }

            return $"SELECT {selectorsRepr} FROM {fromTablesRepr} ";
        }

        protected string WhereQueryString<TData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData record,
            out Action<SqlCommand> addCmdParams)
            where TData : IDisassembleable
        {
            return WhereQueryString<TData, TData>(
                conditions,
                record,
                out addCmdParams);
        }

        protected string WhereQueryString<TData, CTData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> conditions,
            TData record,
            out Action<SqlCommand> addCmdParams)
            where TData : IDisassembleable
            where CTData : TData
        {
            TableAPI table = _tablesByType[typeof(CTData)];
            table.StartSession();

            IDictionary<string, SqlParameter> cmdParams = conditions
                .ToDictionary(
                    c => c.ColumnName,
                    c => table.ExtractParameter(record, c.ValueFactory, c.ColumnName));
            addCmdParams = (cmd) => AddExtractedParametersToCommand(cmd, cmdParams.Values);

            return "WHERE " + string.Join(',', conditions.Select(
                c => $"{table.Name}.{c.ColumnName} = {cmdParams[c.ColumnName].ParameterName}"));
        }

        protected void AddExtractedParametersToCommand(
            SqlCommand cmd,
            IEnumerable<SqlParameter> cmdParams)
        {
            cmd.Parameters
                .AddRange(cmdParams
                    .Where(p => !cmd.Parameters.Contains(p.ParameterName))
                    .ToArray());

            return;
        }

        protected string UpdateQueryString<TData>(
            IEnumerable<(string ColumnName, Func<TData, object> ValueFactory)> setters,
            TData record,
            out Action<SqlCommand> addCmdParams)
            where TData : IDisassembleable
        {
            TableAPI table = _tablesByType[typeof(TData)];
            table.StartSession();

            IDictionary<string, SqlParameter> cmdParams = setters
                .ToDictionary(
                    c => c.ColumnName,
                    c => table.ExtractParameter(record, c.ValueFactory, c.ColumnName));
            addCmdParams = (cmd) => AddExtractedParametersToCommand(cmd, cmdParams.Values);

            return $"UPDATE {table.Name} SET {string.Join(',', setters.Select(
                c => $"{c.ColumnName} = {cmdParams[c.ColumnName].ParameterName}"))} ";
        }

        protected string DeleteQueryString<TData>()
            where TData : IDisassembleable
        {
            TableAPI table = _tablesByType[typeof(TData)];
            table.StartSession();

            return $"DELETE FROM {table.Name} ";
        }

        protected string OrderByQueryString<TData>(
            IEnumerable<string> orderingConditions)
            where TData : IDisassembleable
        {
            TableAPI table = _tablesByType[typeof(TData)];
            table.StartSession();

            return $"ORDER BY {string.Join(',', orderingConditions)} ";
        }
    }
}
