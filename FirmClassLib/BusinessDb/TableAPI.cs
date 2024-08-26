using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;

namespace FirmClassLib.BusinessDb
{
    public class TableAPI
    {
        public string Name { get; private set; }

        public string[] Columns { get; private set; }

        private IDictionary<string, SqlParameter> _paramsByColumn;

        //public IReadOnlyDictionary<string, SqlParameter> ParamsByColumn
        //{ get => _paramsByColumn.AsReadOnly(); }

        /// <summary>
        /// Информация о связи "параметры-источники их появления в запросе".
        /// Запрос может быть вида:
        /// UPDATE table
        /// SET Name=@Name1
        /// WHERE Name=@Name2
        /// Чтобы различить параметр 1 от параметра 2, нужно 
        /// различить источник параметра 1 (паттерн 1) 
        /// от источника параметра 2 (паттерн 2).
        /// </summary>
        private IDictionary<string, IList<object>> _sessionData;

        public TableAPI(string name, string[] columns, SqlDbType[] columnTypes)
        {
            if (columns.Length != columnTypes.Length)
            {
                throw new Exception("Длины массива столбцов и массива соответствующих типов не совпадают.");
            }

            Name = name;
            Columns = columns;
            _paramsByColumn = Columns
                .Zip(columnTypes)
                .ToDictionary(
                    ((string Column, SqlDbType ColType) info) =>
                        info.Column,
                    ((string Column, SqlDbType ColType) info) => 
                        new SqlParameter($"@{info.Column}", info.ColType));
            _paramsByColumn.Add("Id", new SqlParameter("@Id", SqlDbType.Int));

            return;
        }

        public string GetColumnsRepr()
        {
            return $"({string.Join(',', Columns)})";
        }

        public void StartSession()
        {
            if (_sessionData is null)
                _sessionData = new Dictionary<string, IList<object>>();

            return;
        }

        public void ResetSession()
        {
            if (_sessionData is null) return;

            foreach (var cmdParamPattern in _sessionData.Keys)
            {
                _sessionData[cmdParamPattern].Clear();
            }
            _sessionData.Clear();

            return;
        }

        private int GetParameterId(SqlParameter cmdParam, object source)
        {
            if (!_sessionData.ContainsKey(cmdParam.ParameterName))
            {
                _sessionData.Add(
                    cmdParam.ParameterName, 
                    new List<object>(2) { source });
                return 0;
            }

            IList<object> sources = _sessionData[cmdParam.ParameterName];
            int i;

            for (i = 0; i < sources.Count; i++)
            {
                if (sources[i].Equals(source)) return i;
            }
            sources.Add(source);

            return i;
        }

        private string GetParameterName(SqlParameter cmdParam, object source)
        {
            return $"{cmdParam.ParameterName}{GetParameterId(cmdParam, source)}";
        }

        public SqlParameter ExtractIdOutputParameter<TData>(
            TData record,
            Func<TData, object> valueFactory)
            where TData : IDisassembleable
        {
            SqlParameter idParam = ExtractParameter(record, valueFactory, "Id");
            idParam.Direction = ParameterDirection.Output;

            return idParam;
        }

        public SqlParameter ExtractParameter<TData>(
            TData record,
            Func<TData, object> valueFactory,
            string column)
            where TData : IDisassembleable
        {
            SqlParameter pattern = _paramsByColumn[column];

            return new SqlParameter(
                GetParameterName(pattern, record), 
                pattern.SqlDbType)
                { Value = valueFactory(record) };
        }

        public IEnumerable<SqlParameter> ExtractParameters<TData>(
            TData record)
            where TData : IDisassembleable
        {
            IEnumerable<object> colValues = record.Disassemble();

            return Columns
                .Zip(colValues)
                .Select(
                    ((string Column, object ColValue) info) => 
                        (info.ColValue, _paramsByColumn[info.Column]))
                .Select(
                    ((object Value, SqlParameter Pattern) info) =>
                        new SqlParameter(
                            GetParameterName(info.Pattern, record), 
                            info.Pattern.SqlDbType)
                        { Value = info.Value });
        }

        public IEnumerable<SqlParameter[]> ExtractParameters<TData>(
            IEnumerable<TData> records)
            where TData : IDisassembleable
        {
            return (records.Select(r => ExtractParameters(r).ToArray())).ToArray();
        }
    }
}
