/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Data;
using Npgsql;

namespace Common.Postgres
{
    public class Database
    {
        private NpgsqlConnection _conn;

        public NpgsqlConnection Connection
        {
            get { return _conn; }
            set { _conn = value; }
        }

        public Database(string connectionString)
        {
            _conn = new NpgsqlConnection(connectionString);
        }

        public static NpgsqlConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public void Open()
        {
            if (!IsConnectionOpen())
                _conn.Open();
        }

        public void Reestablish()
        {
            try
            {
                _conn.Close();
            }
            catch (Exception)
            {
            }

            _conn = new NpgsqlConnection(_conn.ConnectionString);
            _conn.Open();
        }

        public static void Open(NpgsqlConnection conn)
        {
            if (!IsConnectionOpen(conn))
                conn.Open();
        }

        public void Close()
        {
            if (IsConnectionOpen())
                _conn.Close();
        }

        public static void Close(NpgsqlConnection conn)
        {
            if (IsConnectionOpen(conn))
                conn.Close();
        }

        public bool IsConnectionOpen()
        {
            if (_conn == null) return false;
            return _conn.State == ConnectionState.Open;
        }

        public static bool IsConnectionOpen(NpgsqlConnection conn)
        {
            if (conn == null) return false;
            return conn.State == ConnectionState.Open;
        }

        /// <summary>
        /// Executes a query against the database without accepting results.  This method will open the connection if needed but
        /// will only close the connection if it opened the connection.
        /// </summary>
        /// <param name="query">Query to execute</param>
        public void DBExec(string query)
        {
            bool wasOpen = true;
            NpgsqlCommand cmd;

            if (_conn.State != ConnectionState.Open)
            {
                wasOpen = false;
                _conn.Open();
            }

            cmd = new NpgsqlCommand(query, _conn);

            cmd.ExecuteNonQuery();

            if (!wasOpen)
                _conn.Close();
        }

        public void DBExec(NpgsqlCommand cmd)
        {
            if (cmd.Connection == null || cmd.Connection.State != ConnectionState.Open)
                cmd.Connection = _conn;

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a query against the database returning the identity value.  This method will open the connection if 
        /// needed but will only close the connection if it opened the connection.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="sequencename">The name of the sequence to get the identity from</param>
        /// <returns>A Int64 with the identity value</returns>
        public Int64 DBExec(string query, string sequencename)
        {
            bool wasOpen = true;
            NpgsqlCommand cmd;
            DataTable dt = new DataTable();
            NpgsqlDataAdapter da;

            if (_conn.State != ConnectionState.Open)
            {
                wasOpen = false;
                _conn.Open();
            }

            cmd = new NpgsqlCommand(query, _conn);

            cmd.ExecuteNonQuery();

            da = new NpgsqlDataAdapter("SELECT last_value AS \"Identity\" FROM \"" + sequencename + "\"", _conn);
            da.Fill(dt);

            if (!wasOpen)
                _conn.Close();

            if (dt.Rows.Count > 0)
                return (Int64)dt.Rows[0]["Identity"];
            return -1;
        }

        /// <summary>
        /// Executes a query against the database and returns a DataTable of results.  This method will open the connection
        /// if needed but will only close the connection if it opened the connection.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <returns>A DataTable of results</returns>
        public DataTable GetTable(string query)
        {
            DataTable dt = new DataTable();
            bool wasOpen = true;
            NpgsqlCommand cmd;
            NpgsqlDataAdapter da;

            if (_conn.State != ConnectionState.Open)
            {
                wasOpen = false;
                _conn.Open();
            }

            cmd = new NpgsqlCommand(query, _conn);
            da = new NpgsqlDataAdapter(cmd);
            da.Fill(dt);

            if (!wasOpen)
                _conn.Close();

            return dt;
        }

        public DataTable GetTable(NpgsqlCommand cmd)
        {
            DataTable dt = new DataTable();
            NpgsqlDataAdapter da;

            if (cmd.Connection == null)
            {
                cmd.Connection = new NpgsqlConnection(SettingsBase.Instance.PostgresConnectionString);
            }

            da = new NpgsqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public static DataTable GetTable(NpgsqlConnection conn, NpgsqlCommand cmd)
        {
            DataTable dt = new DataTable();
            NpgsqlDataAdapter da;

            cmd.Connection = conn;

            da = new NpgsqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public static string ParseIn(string str)
        {
            if (str == null)
                return null;

            str = str.Replace("'", "&39;");
            str = str.Replace("\"", "&34;");
            str = str.Replace("\\", "&92;");
            str = str.Replace("%", "&37;");

            return str;
        }

        public static string ParseOut(string str)
        {
            if (str == null)
                return null;

            str = str.Replace("&39;", "'");
            str = str.Replace("&34;", "\"");
            str = str.Replace("&92;", "\\");
            str = str.Replace("&37;", "%");

            return str;
        }

        public static string DetermineColumnName(DataRow dr, string possibility1, string possibility2,
            string exceptionMessage)
        {
            if (!dr.Table.Columns.Contains(possibility1))
            {
                if (!dr.Table.Columns.Contains(possibility2))
                    throw new Exception(exceptionMessage);
                else
                    return possibility2;
            }
            else
                return possibility1;
        }
    }
}
