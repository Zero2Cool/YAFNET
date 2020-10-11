/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bj�rnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2020 Ingo Herbote
 * https://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * https://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Data.MsSql.Functions
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using ServiceStack.OrmLite;

    using YAF.Configuration;
    using YAF.Core.Data;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Extensions.Data;
    using YAF.Types.Interfaces.Data;

    /// <summary>
    /// MS SQL Specific Functions
    /// </summary>
    public static class MsSqlSpecificFunctions
    {
        /// <summary>
        /// Gets the database size
        /// </summary>
        /// <param name="dbAccess">The database access.</param>
        /// <returns>
        /// integer value for database size
        /// </returns>
        public static int DBSize(this IDbAccess dbAccess)
        {
            CodeContracts.VerifyNotNull(dbAccess, "dbAccess");

            return dbAccess.Execute(
                db => db.Connection.Scalar<int>("SELECT sum(reserved_page_count) * 8.0 / 1024 FROM sys.dm_db_partition_stats"));
        }

        /// <summary>
        /// Gets the current SQL Engine Edition.
        /// </summary>
        /// <param name="dbAccess">The database access.</param>
        /// <returns>
        /// Returns the current SQL Engine Edition.
        /// </returns>
        public static string GetSQLVersion(this IDbAccess dbAccess)
        {
            CodeContracts.VerifyNotNull(dbAccess, "dbAccess");

            try
            {
                return dbAccess.Execute(
                    db => db.Connection.Scalar<string>("select @@version"));
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// The shrink database.
        /// </summary>
        /// <param name="dbAccess">
        /// The db access.
        /// </param>
        public static void ShrinkDatabase(this IDbAccess dbAccess)
        {
            CodeContracts.VerifyNotNull(dbAccess, "dbAccess");

            dbAccess.Execute(db => db.Connection.ExecuteSql($"DBCC SHRINKDATABASE(N'{db.Connection.Database}')"));
        }

        /// <summary>
        /// The re index database.
        /// </summary>
        /// <param name="dbAccess">
        /// The db access.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ReIndexDatabase(this IDbAccess dbAccess)
        {
            CodeContracts.VerifyNotNull(dbAccess, "dbAccess");

            var sb = new StringBuilder();

            sb.AppendLine("DECLARE @MyTable VARCHAR(255)");
            sb.AppendLine("DECLARE myCursor");
            sb.AppendLine("CURSOR FOR");
            sb.AppendFormat(
                "SELECT table_name FROM information_schema.tables WHERE table_type = 'base table' AND table_name LIKE '{0}%'",
                Config.DatabaseObjectQualifier);
            sb.AppendLine("OPEN myCursor");
            sb.AppendLine("FETCH NEXT");
            sb.AppendLine("FROM myCursor INTO @MyTable");
            sb.AppendLine("WHILE @@FETCH_STATUS = 0");
            sb.AppendLine("BEGIN");
            sb.AppendLine("PRINT 'Reindexing Table:  ' + @MyTable");
            sb.AppendLine("DBCC DBREINDEX(@MyTable, '', 80)");
            sb.AppendLine("FETCH NEXT");
            sb.AppendLine("FROM myCursor INTO @MyTable");
            sb.AppendLine("END");
            sb.AppendLine("CLOSE myCursor");
            sb.AppendLine("DEALLOCATE myCursor");

            return dbAccess.Execute(
                db => db.Connection.Scalar<string>(sb.ToString()));
        }

        /// <summary>
        /// System initialize and execute script's.
        /// </summary>
        /// <param name="dbAccess">
        /// The db Access.
        /// </param>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="scriptFile">
        /// The script file.
        /// </param>
        /// <param name="useTransactions">
        /// The use transactions.
        /// </param>
        public static void SystemInitializeExecuteScripts(
            this IDbAccess dbAccess,
            [NotNull] string script,
            [NotNull] string scriptFile,
            bool useTransactions)
        {
            script = CommandTextHelpers.GetCommandTextReplaced(script);

            var statements = Regex.Split(script, "\\sGO\\s", RegexOptions.IgnoreCase).ToList();

            // use transactions...
            if (useTransactions)
            {
                using (var trans = dbAccess.CreateConnectionOpen().BeginTransaction())
                {
                    foreach (var sql in statements.Select(sql0 => sql0.Trim()))
                    {
                        try
                        {
                            if (sql.ToLower().IndexOf("setuser", StringComparison.Ordinal) >= 0)
                            {
                                continue;
                            }

                            if (sql.Length <= 0)
                            {
                                continue;
                            }

                            using (var cmd = trans.Connection.CreateCommand())
                            {
                                // added so command won't timeout anymore...
                                cmd.CommandTimeout = int.Parse(Config.SqlCommandTimeout);
                                cmd.Transaction = trans;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = sql.Trim();
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception x)
                        {
                            trans.Rollback();
                            throw new Exception($"FILE:\n{scriptFile}\n\nERROR:\n{x.Message}\n\nSTATEMENT:\n{sql}");
                        }
                    }

                    trans.Commit();
                }
            }
            else
            {
                // don't use transactions
                foreach (var sql in statements.Select(sql0 => sql0.Trim()))
                {
                    try
                    {
                        if (sql.ToLower().IndexOf("setuser", StringComparison.Ordinal) >= 0)
                        {
                            continue;
                        }

                        if (sql.Length <= 0)
                        {
                            continue;
                        }

                        dbAccess.Execute(db => db.Connection.Scalar<string>(sql.Trim()));
                    }
                    catch (Exception x)
                    {
                        throw new Exception($"FILE:\n{scriptFile}\n\nERROR:\n{x.Message}\n\nSTATEMENT:\n{sql}");
                    }
                }
            }
        }

        /// <summary>
        /// The db_recovery_mode.
        /// </summary>
        /// <param name="dbAccess">
        /// The db Access.
        /// </param>
        /// <param name="recoveryMode">
        /// The recovery mode.
        /// </param>
        public static string ChangeRecoveryMode(this IDbAccess dbAccess, [NotNull] string recoveryMode)
        {
            try
            {
                var recoveryModeSql =
                    $"ALTER DATABASE {dbAccess.CreateConnectionOpen().Database} SET RECOVERY {recoveryMode}";

                return dbAccess.Execute(
                    db => db.Connection.Scalar<string>(recoveryModeSql));
            }
            catch (Exception error)
            {
                var expressDb = string.Empty;
                if (error.Message.ToUpperInvariant().Contains("'SET'"))
                {
                    expressDb = "MS SQL Server Express Editions are not supported by the application.";
                }

                return $"\r\n{error.Message}\r\n{expressDb}";
            }
        }

        private static string messageRunSql;

        /// <summary>
        /// Run SQL
        /// </summary>
        /// <param name="dbAccess">
        /// The db Access.
        /// </param>
        /// <param name="sql">
        /// The sql.
        /// </param>
        /// <param name="useTransaction">
        /// The use Transaction.
        /// </param>
        public static string RunSQL(this IDbAccess dbAccess, [NotNull] string sql, bool useTransaction)
        {
            try
            {
                using (var trans = dbAccess.CreateConnectionOpen().BeginTransaction())
                {
                    sql = CommandTextHelpers.GetCommandTextReplaced(sql.Trim());

                    using (var cmd = trans.Connection.CreateCommand())
                    {
                        // added so command won't timeout anymore...
                        cmd.CommandTimeout = int.Parse(Config.SqlCommandTimeout);
                        cmd.Transaction = trans;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;

                        return InnerRunSqlExecuteReader(cmd as SqlCommand, useTransaction);
                    }
                }
            }
            finally
            {
                messageRunSql = string.Empty;
            }
        }

        /// <summary>
        /// Called from RunSql -- just runs a sql command according to specifications.
        /// </summary>
        /// <param name="command">
        /// </param>
        /// <param name="useTransaction">
        /// </param>
        /// <returns>
        /// The inner run sql execute reader.
        /// </returns>
        [NotNull]
        private static string InnerRunSqlExecuteReader([NotNull] SqlCommand command, bool useTransaction)
        {
            SqlDataReader reader = null;
            var results = new StringBuilder();

            try
            {
                try
                {
                    command.Transaction = useTransaction ? command.Transaction : null;
                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        var rowIndex = 1;
                        var columnNames = reader.GetSchemaTable().Rows.Cast<DataRow>()
                            .Select(r => r["ColumnName"].ToString()).ToList();

                        results.Append("RowNumber");

                        columnNames.ForEach(
                            n =>
                            {
                                results.Append(",");
                                results.Append(n);
                            });

                        results.AppendLine();

                        while (reader.Read())
                        {
                            results.AppendFormat(@"""{0}""", rowIndex++);

                            // dump all columns...
                            columnNames.ForEach(
                                col => results.AppendFormat(
                                    @",""{0}""",
                                    reader[col].ToString().Replace("\"", "\"\"")));

                            results.AppendLine();
                        }
                    }
                    else if (reader.RecordsAffected > 0)
                    {
                        results.AppendFormat("{0} Record(s) Affected", reader.RecordsAffected);
                        results.AppendLine();
                    }
                    else
                    {
                        if (messageRunSql.IsSet())
                        {
                            results.AppendLine(messageRunSql);
                            results.AppendLine();
                        }

                        results.AppendLine("No Results Returned.");
                    }

                    reader.Close();

                    command.Transaction?.Commit();
                }
                finally
                {
                    command.Transaction?.Rollback();
                }
            }
            catch (Exception x)
            {
                reader?.Close();

                results.AppendLine();
                results.AppendFormat("SQL ERROR: {0}", x);
            }

            return results.ToString();
        }
    }
}