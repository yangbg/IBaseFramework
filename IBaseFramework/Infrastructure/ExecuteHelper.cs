using System;
using System.Data;
using System.Data.SqlClient;
using IBaseFramework.Diagnostics;
using MySql.Data.MySqlClient;

namespace IBaseFramework.Infrastructure
{
    public class ExecuteHelper
    {
        [ThreadStatic]
        static IDbTransaction _dbTransaction;
        public static void Transaction(Action action)
        {
            if (_dbTransaction != null)
            {
                action();
            }
            else
            {
                TransactionInner(action);
            }
        }

        private static void TransactionInner(Action action)
        {
            var dbConnection = ConnectionFactory.CreateConnection(DapperContext.DatabaseConfiguration);
            _dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                action();

                _dbTransaction.Commit();
            }
            catch (Exception)
            {
                _dbTransaction.Rollback();
                throw;
            }
            finally
            {
                _dbTransaction?.Dispose();
                dbConnection?.Close();
                _dbTransaction = null;
            }
        }

        public static TResult Execute<TResult>(Func<IDbConnection, TResult> func)
        {
            var dbConnection = ConnectionFactory.CreateConnection(DapperContext.DatabaseConfiguration);

            try
            {
                return func(dbConnection);
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.ExecuteError(ex);

                throw;
            }
            finally
            {
                if (_dbTransaction == null)
                {
                    DiagnosticsHelper.ExecuteDispose(dbConnection);

                    dbConnection?.Close();
                }
            }
        }
    }

    public class ConnectionFactory
    {
        [ThreadStatic]
        private static IDbConnection _dbConnection;
        public static IDbConnection CreateConnection(DapperContextConfig dapperContext)
        {
            if (_dbConnection != null)
            {
                if (_dbConnection.State != ConnectionState.Open && _dbConnection.State != ConnectionState.Connecting)
                    _dbConnection.Open();

                return _dbConnection;
            }

            switch (dapperContext.SqlProvider)
            {
                case SqlProvider.SQLServer:
                    _dbConnection = new SqlConnection(dapperContext.ConnectionString);
                    break;
                case SqlProvider.MySQL:
                    _dbConnection = new MySqlConnection(dapperContext.ConnectionString);
                    break;
                default:
                    _dbConnection = new MySqlConnection(dapperContext.ConnectionString);
                    break;
            }

            if (_dbConnection.State != ConnectionState.Open && _dbConnection.State != ConnectionState.Connecting)
                _dbConnection.Open();

            return _dbConnection;
        }
    }
}