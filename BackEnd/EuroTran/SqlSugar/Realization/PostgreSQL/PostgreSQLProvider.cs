using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
namespace SqlSugar
{
    public class PostgreSQLProvider : AdoProvider
    {
        public PostgreSQLProvider() { }
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        var mySqlConnectionString = base.Context.CurrentConnectionConfig.ConnectionString;
                        if (!mySqlConnectionString.ToLower().Contains("charset"))
                        {
                            mySqlConnectionString = mySqlConnectionString.Trim().TrimEnd(';') + ";charset=utf8;";
                        }
                        base._DbConnection = new MySqlConnection(mySqlConnectionString);
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
            return new MySqlDataAdapter();
        }
        public override IDbCommand GetCommand(string sql, SugarParameter[] parameters)
        {
            var sqlCommand = new MySqlCommand(sql, (MySqlConnection)this.Connection)
            {
                CommandType = this.CommandType,
                CommandTimeout = this.CommandTimeOut
            };
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (MySqlTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                var ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((MySqlParameter[])ipars);
            }
            CheckConnection();
            return sqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, IDbCommand command)
        {
            ((MySqlDataAdapter)dataAdapter).SelectCommand = (MySqlCommand)command;
        }
        /// <summary>
        /// if mysql return MySqlParameter[] pars
        /// if sqlerver return SqlParameter[] pars ...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IDataParameter[] ToIDbDataParameter(params SugarParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            var result = new MySqlParameter[parameters.Length];
            var index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new MySqlParameter
                {
                    ParameterName = parameter.ParameterName,
                    Size = parameter.Size,
                    Value = parameter.Value,
                    DbType = parameter.DbType,
                    Direction = parameter.Direction
                };
                if (sqlParameter.Direction == 0)
                {
                    sqlParameter.Direction = ParameterDirection.Input;
                }
                result[index] = sqlParameter;
                if (sqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == sqlParameter.ParameterName);
                    this.OutputParameters.Add(sqlParameter);
                }
                ++index;
            }
            return result;
        }
    }
}
