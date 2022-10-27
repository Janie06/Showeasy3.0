using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace SqlSugar
{
    public class SqlServerProvider : AdoProvider
    {
        public SqlServerProvider() { }
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        base._DbConnection = new SqlConnection(base.Context.CurrentConnectionConfig.ConnectionString);
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
        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="transactionName"></param>
        public override void BeginTran(string transactionName)
        {
            CheckConnection();
            base.Transaction = ((SqlConnection)this.Connection).BeginTransaction(transactionName);
        }
        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="iso"></param>
        /// <param name="transactionName"></param>
        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            CheckConnection();
            base.Transaction = ((SqlConnection)this.Connection).BeginTransaction(iso, transactionName);
        }
        public override IDataAdapter GetAdapter()
        {
            return new SqlDataAdapter();
        }
        public override IDbCommand GetCommand(string sql, SugarParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(sql, (SqlConnection)this.Connection)
            {
                CommandType = this.CommandType,
                CommandTimeout = this.CommandTimeOut
            };
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (SqlTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                var ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((SqlParameter[])ipars);
            }
            CheckConnection();
            return sqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, IDbCommand command)
        {
            ((SqlDataAdapter)dataAdapter).SelectCommand = (SqlCommand)command;
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
            var result = new SqlParameter[parameters.Length];
            var index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new SqlParameter
                {
                    ParameterName = parameter.ParameterName,
                    UdtTypeName = parameter.UdtTypeName,
                    Size = parameter.Size,
                    Value = parameter.Value,
                    DbType = parameter.DbType,
                    Direction = parameter.Direction
                };
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
