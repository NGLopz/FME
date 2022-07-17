using FME.Repository.Dao.Common;
using FME.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.Implementation.SqlServer
{
    public class SqlConnector : IConnector
    {
        private readonly string _connString;

        private SqlConnection _currentConnection;

        public SqlConnector(IConfiguration appSettings, string pConnStr = "")
        {
            AppSettings = appSettings;
            _connString = pConnStr == string.Empty ? AppSettings.GetConnectionString("DBConexion") : pConnStr;
        }

        private IConfiguration AppSettings { get; }

        public SqlTransaction BeginSqlTransaction()
        {
            try
            {
                _currentConnection = OpenConnection();
                var transaction = _currentConnection.BeginTransaction();

                return transaction;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error creando la transacción en la base de datos", sqlEx);
            }
        }

        public void CloseConnection()
        {
            if (_currentConnection != null && _currentConnection.State != ConnectionState.Closed)
            {
                _currentConnection.Close();
                _currentConnection.Dispose();
            }
        }

        public Task<DataTable> ExecuteQueryForDataTable(string query, List<SqlParameter> sqlParameters = null, bool isStoreProcedure = false)
        {
            var conn = new SqlConnection(_connString);
            try
            {
                return Task.Run(() =>
                {
                    var dt = new DataTable();
                    using (conn)
                    {
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = isStoreProcedure ? CommandType.StoredProcedure : CommandType.Text;
                            cmd.Parameters.AddRange(sqlParameters?.ToArray());
                            var adapter = new SqlDataAdapter(cmd);
                            _ = adapter.Fill(dt);
                        }
                    }

                    return dt;
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public bool ExecuteNonQuery(string query, List<SqlParameter> sqlParameters = null, bool isStoreProcedure = false)
        {
            var conn = new SqlConnection(_connString);
            try
            {
                using (conn)
                {
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = isStoreProcedure ? CommandType.StoredProcedure : CommandType.Text;
                        cmd.Parameters.AddRange(sqlParameters?.ToArray());
                        conn.Open();
                        _ = cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public long ExecuteScalar(string query, SqlTransaction transaction, List<SqlParameter> sqlParameters = null, bool isStoreProcedure = false)
        {
            return transaction is null
                ? ExecuteScalar(query, sqlParameters, isStoreProcedure)
                : ExecuteScalarTransaction(query, transaction, sqlParameters, isStoreProcedure);
        }
        /// <summary>
        /// Ejecuta un SP que recibira como parametro un json y algun otro parametro opcional.
        /// </summary>
        /// <param name="query">SP a ejecutar</param>
        /// <param name="input">Objeto que sera serializado a json y servira como entrada al sp.</param>
        /// <param name="sqlParameters">Parametros sql adicionales (opcional).</param>
        /// <param name="isStoreProcedure">Indica si se ejecutara un SP.</param>
        /// <returns>Escalar retornado por la consulta.</returns>
        public DatabaseResult ExecuteWithJsonInput(string query, object input, List<SqlParameter> sqlParameters = null)
        {
            var paramList = new List<SqlParameter>()
            {
                new SqlParameter("Data", JObject.FromObject(input).ToString())
            };
            if (sqlParameters != null)
            {
                paramList.AddRange(sqlParameters);
            }

            int intento = 0;
            var result = new DatabaseResult();
            DbDataReader reader;

        RETRY:

            SqlConnection cname = OpenConnection(_connString);
            try
            {
                using (cname)
                {
                    using (SqlCommand cmd = new SqlCommand(query, cname))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        FillParameters(paramList, cmd);
                        reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            _ = reader.Read();
                            result = new DatabaseResult()
                            {
                                EntityId = (long)reader.GetValue("Id"),
                                IdResponseCode = (int)reader.GetValue("IdCodigoRespuesta"),
                                Message = Convert.ToString(reader.GetValue("MensajeError")),
                                ResponseCode = Convert.ToString(reader.GetValue("CodigoRespuesta")),
                                Table = Convert.ToString(reader.GetValue("NombreTabla"))
                            };
                        }

                        reader.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                if (++intento < 2)
                {
                    goto RETRY;
                }

                throw new Exception(e.Message, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Error ejecutando consulta [" + query + "] en GetJson", ex);
            }
            finally
            {
                if (cname.State == ConnectionState.Open)
                    cname.Close();
            }

            return result;
        }

        public DatabaseResult ExecuteWithJsonInputList(string query, object input, List<SqlParameter> sqlParameters = null)
        {
            var paramList = new List<SqlParameter>()
            {
                new SqlParameter("Data", JsonConvert.SerializeObject(input).ToString())
            };
            if (sqlParameters != null)
            {
                paramList.AddRange(sqlParameters);
            }

            int intento = 0;
            var result = new DatabaseResult();
            DbDataReader reader;

        RETRY:

            SqlConnection cname = OpenConnection(_connString);
            try
            {
                using (cname)
                {
                    using (SqlCommand cmd = new SqlCommand(query, cname))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        FillParameters(paramList, cmd);
                        reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            _ = reader.Read();
                            result = new DatabaseResult()
                            {
                                EntityId = (long)reader.GetValue("Id"),
                                IdResponseCode = (int)reader.GetValue("IdCodigoRespuesta"),
                                Message = Convert.ToString(reader.GetValue("MensajeError")),
                                ResponseCode = Convert.ToString(reader.GetValue("CodigoRespuesta")),
                                Table = Convert.ToString(reader.GetValue("NombreTabla"))
                            };
                        }

                        reader.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                if (++intento < 2)
                {
                    goto RETRY;
                }

                throw new Exception(e.Message, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Error ejecutando consulta [" + query + "] en GetJson", ex);
            }
            finally
            {
                if (cname.State == ConnectionState.Open)
                    cname.Close();
            }

            return result;
        }


        /// <summary>
        /// Ejecuta un SP que recibira como parametro.
        /// </summary>
        /// <param name="query">SP a ejecutar</param>
        /// <param name="input">Objeto que sera serializado a json y servira como entrada al sp.</param>
        /// <param name="sqlParameters">Parametros sql adicionales (opcional).</param>
        /// <param name="isStoreProcedure">Indica si se ejecutara un SP.</param>
        /// <returns>Escalar retornado por la consulta.</returns>
        public DatabaseResult ExecuteWithJsonInput(string query, List<SqlParameter> sqlParameters)
        {
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (sqlParameters != null)
            {
                paramList.AddRange(sqlParameters);
            }

            int intento = 0;
            DatabaseResult result = new DatabaseResult();
            DbDataReader reader;

        RETRY:

            SqlConnection cname = OpenConnection(this._connString);
            try
            {
                using (cname)
                {
                    using SqlCommand cmd = new SqlCommand(query, cname)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    FillParameters(paramList, cmd);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        _ = reader.Read();
                        result = new DatabaseResult()
                        {
                            EntityId = (long)reader.GetValue("Id"),
                            IdResponseCode = (int)reader.GetValue("IdCodigoRespuesta"),
                            Message = Convert.ToString(reader.GetValue("MensajeError")),
                            ResponseCode = Convert.ToString(reader.GetValue("CodigoRespuesta")),
                            Table = Convert.ToString(reader.GetValue("NombreTabla"))
                        };
                    }

                    reader.Close();
                }
            }
            catch (SqlException e)
            {
                if (++intento < 2)
                {
                    goto RETRY;
                }

                throw new Exception(e.Message, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Error ejecutando consulta [" + query + "] en GetJson", ex);
            }
            finally
            {
                if (cname.State == ConnectionState.Open)
                    cname.Close();
            }

            return result;
        }

        /// <summary>Para obtener dataser desde un SP que se ejecuta en la base de datos.</summary>
        /// <param name="tableName">Name de la tabla con que estara en el Dataset.</param>
        /// <param name="spName">Name del StoredProcdure.</param>
        /// <param name="parameters">Lista de SQLPARAMETER necesarios para el SP.</param>
        /// <returns>The resulting data set.</returns>
        public DataSet CreateDs(string tableName, string spName, List<SqlParameter> parameters)
        {
            var retry = 0;
            var ds = new DataSet();

        RETRY:

            var cname = OpenConnection(_connString);
            try
            {
                using (cname)
                {
                    using (var cmd = new SqlCommand(spName, cname))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        FillParameters(parameters, cmd);

                        var adapter = new SqlDataAdapter(cmd);
                        _ = adapter.Fill(ds, tableName);
                    }
                }
            }
            catch (SqlException e)
            {
                if (++retry < 2)
                {
                    goto RETRY;
                }

                throw new Exception(e.Message, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Error ejecutando consulta [" + spName + "] en CreaDS", ex);
            }
            finally
            {
                cname.Close();
            }

            return ds;
        }

        public Task<bool> ExecuteNonQueryTask(string query, List<SqlParameter> sqlParameters = null, bool isStoreProcedure = false)
        {
            var conn = new SqlConnection(_connString);
            try
            {
                return Task.Run(() =>
                {
                    using (conn)
                    {
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = isStoreProcedure ? CommandType.StoredProcedure : CommandType.Text;
                            cmd.Parameters.AddRange(sqlParameters?.ToArray());
                            conn.Open();
                            _ = cmd.ExecuteNonQuery();
                        }
                    }

                    return true;
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public string GetJson(string spName, JObject jsonParams)
        {
            return GetJson(spName, JObjectToSqlParams(jsonParams));
        }

        /// <summary>
        /// Para obtener resultado en formato Json desde un SP que se ejecuta en la base de datos
        /// </summary>
        /// <param name="spName">Name del StoredProcdure </param>
        /// <param name="parameters">Lista de SQLPARAMETER necesarios para el SP </param>
        /// <returns>JsonString.</returns>
        public string GetJson(string spName, List<SqlParameter> parameters)
        {
            int intento = 0;
            var jsonResult = new StringBuilder();
            DbDataReader reader;

        RETRY:

            SqlConnection cname = OpenConnection(_connString);
            try
            {
                using (cname)
                {
                    using (SqlCommand cmd = new SqlCommand(spName, cname))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        FillParameters(parameters, cmd);
                        reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _ = jsonResult.Append(reader.GetValue(0).ToString());
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                if (++intento < 2)
                {
                    goto RETRY;
                }

                throw new Exception(e.Message, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Error ejecutando consulta [" + spName + "] en GetJson", ex);
            }
            finally
            {
                cname.Close();
            }

            return jsonResult.ToString();
        }

        /// <summary>
        ///     Llena los valores de los parametros de la lista.
        /// </summary>
        /// <param name="parameters">The sql parameters.</param>
        /// <param name="cmd">The sql command.</param>
        private static void FillParameters(List<SqlParameter> parameters, SqlCommand cmd)
        {
            parameters.ForEach(param => { _ = cmd.Parameters.AddWithValue(param.ParameterName, param.Value); });
        }

        private SqlConnection OpenConnection(string connString = "")
        {
            if (string.IsNullOrEmpty(connString))
            {
                connString = _connString;
            }

            var myConn = new SqlConnection(connString);
            var retryAttempts = 1;
        retry:
            try
            {
                myConn.Open();
            }
            catch (SqlException sqlEx)
            {
                if (retryAttempts < 3)
                {
                    retryAttempts++;
                    goto retry;
                }

                throw new Exception("Error conectando a la base de datos", sqlEx);
            }

            return myConn;
        }

        private long ExecuteScalar(string query, List<SqlParameter> sqlParameters = null, bool isStoreProcedure = false)
        {
            var conn = new SqlConnection(_connString);
            try
            {
                using (conn)
                {
                    long @return;
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = isStoreProcedure ? CommandType.StoredProcedure : CommandType.Text;
                        cmd.Parameters.AddRange(sqlParameters?.ToArray());
                        conn.Open();
                        @return = (long)cmd.ExecuteScalar();
                    }

                    return @return;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private long ExecuteScalarTransaction(
            string query,
            SqlTransaction transaction,
            List<SqlParameter> sqlParameters = null,
            bool isStoreProcedure = false)
        {
            try
            {
                long @return;
                using (var cmd = new SqlCommand(query, transaction.Connection, transaction))
                {
                    cmd.CommandType = isStoreProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    cmd.Parameters.AddRange(sqlParameters?.ToArray());
                    @return = (long)cmd.ExecuteScalar();
                }

                return @return;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private List<SqlParameter> JObjectToSqlParams(JObject jsonParams)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            foreach (var pair in jsonParams)
            {
                sqlParams.Add(new SqlParameter(pair.Key, pair.Value.ToString()));
            }

            return sqlParams;
        }
        private List<SqlParameter> JObjectToSqlParamsJson(object jsonParams, List<SqlParameter> sqlParameters = null)
        {
            //List<SqlParameter> sqlParams = new List<SqlParameter>();

            var paramList = new List<SqlParameter>()
            {
                new SqlParameter("Data", JsonConvert.SerializeObject(jsonParams).ToString())
            };

            if (sqlParameters != null)
            {
                paramList.AddRange(sqlParameters);
            }
            return paramList;
        }

        public string GetJsonList(string spName, object jsonParams, List<SqlParameter> sqlParameters = null)
        {
            return GetJson(spName, JObjectToSqlParamsJson(jsonParams, sqlParameters));
        }

        public string ExecuteScalarFunctionString(string query)
        {
            SqlConnection conn = new SqlConnection(this._connString);
            try
            {
                using (conn)
                {
                    string @return;
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        @return = (string)cmd.ExecuteScalar();
                    }



                    return @return;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public bool ExecuteScalarFunction(string query)
        {
            SqlConnection conn = new SqlConnection(this._connString);
            try
            {
                using (conn)
                {
                    bool @return;
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        @return = (bool)cmd.ExecuteScalar();
                    }

                    return @return;
                }
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
