using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
namespace SqlSugar
{
    public partial class SqliteProvider : AdoProvider
    {
        public SqliteProvider() { }
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        var SQLiteConnectionString = base.Context.CurrentConnectionConfig.ConnectionString;
                        base._DbConnection = new SQLiteConnection(SQLiteConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Check.Exception(true, ErrorMessage.ConnnectionOpen, ex.Message);
                    }
                }
                return base._DbConnection;
            }
            set
            {
                base._DbConnection = value;
            }
        }
        
        public override void BeginTran(string transactionName)
        {
            base.BeginTran();
        }
        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="iso"></param>
        /// <param name="transactionName"></param>
        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            base.BeginTran(iso);
        }
        public override IDataAdapter GetAdapter()
        {
            return new SQLiteDataAdapter();
        }
        public override IDbCommand GetCommand(string sql, SugarParameter[] parameters)
        {
            var sqlCommand = new SQLiteCommand(sql, (SQLiteConnection)this.Connection)
            {
                CommandType = this.CommandType,
                CommandTimeout = this.CommandTimeOut
            };
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (SQLiteTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                var ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((SQLiteParameter[])ipars);
            }
            CheckConnection();
            return sqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, IDbCommand command)
        {
            ((SQLiteDataAdapter)dataAdapter).SelectCommand = (SQLiteCommand)command;
        }
        /// <summary>
        /// if SQLite return SQLiteParameter[] pars
        /// if sqlerver return SqlParameter[] pars ...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IDataParameter[] ToIDbDataParameter(params SugarParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            var result = new SQLiteParameter[parameters.Length];
            var index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                if (parameter.Value.GetType() == UtilConstants.GuidType)
                {
                    parameter.Value = parameter.Value.ToString();
                }
                var sqlParameter = new SQLiteParameter
                {
                    ParameterName = parameter.ParameterName,
                    Size = parameter.Size,
                    Value = parameter.Value,
                    DbType = parameter.DbType
                };
                result[index] = sqlParameter;
                if (sqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == sqlParameter.ParameterName);
                    this.OutputParameters.Add(sqlParameter);
                }
                if (sqlParameter.DbType == System.Data.DbType.Guid)
                {
                    sqlParameter.DbType = System.Data.DbType.String;
                    sqlParameter.Value = sqlParameter.Value.ObjToString();
                }

                ++index;
            }
            return result;
        }
    }
}
