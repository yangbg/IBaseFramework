using System;
using System.Data;
using System.Data.SqlClient;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;
using IBaseFramework.Ioc;
using MySql.Data.MySqlClient;

namespace IBaseFramework.Infrastructure
{
    public class ExecuteHelper
    {
        [ThreadStatic]
        private static byte _isClose;

        public static void Transaction(Action<IDbTransaction> action)
        {
            _isClose = 1;

            var dbConnection = ConnectionFactory.CreateConnection(DapperContext.DatabaseConfiguration);
            var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                action(dbTransaction);

                dbTransaction.Commit();
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
            finally
            {
                dbTransaction?.Dispose();
                dbConnection?.Close();
                _isClose = 0;
            }
        }

        public static TResult Execute<TResult>(Func<IDbConnection, TResult> func)
        {
            var dbConnection = ConnectionFactory.CreateConnection(DapperContext.DatabaseConfiguration);

            try
            {
                return func(dbConnection);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isClose == 0)
                {
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
                case SqlProvider.MSSQL:
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