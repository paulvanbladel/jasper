﻿using System;
using System.Data.SqlClient;
using System.Reflection;
using Baseline;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Schema
{
    public class SchemaLoader
    {
        private readonly string _connectionString;

        public SchemaLoader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string SchemaName { get; set; } = "dbo";

        public static string ToCreationScript(string schema)
        {
            // TODO -- more here
            return toScript("Creation.sql", schema);
        }

        public static string ToDropScript(string schema)
        {
            return toScript("Drop.sql", schema);
        }

        private static string toScript(string fileName, string schema)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(SchemaLoader), fileName)
                .ReadAllText();

            return text.Replace("%SCHEMA%", schema);
        }

        public void DropAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");
            }
        }

        private void execute(SqlConnection conn, string filename)
        {
            var sql = toScript(filename, SchemaName);

            try
            {
                conn.CreateCommand(sql).ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failure trying to execute:\n\n" + sql, e);
            }
        }

        public void CreateAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Creation.sql");
                execute(conn, "Functions.sql");

            }
        }

        public void RecreateAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");
                execute(conn, "Creation.sql");
                execute(conn, "uspDeleteIncomingEnvelopes.sql");
                execute(conn, "uspDeleteOutgoingEnvelopes.sql");
                execute(conn, "uspDiscardAndReassignOutgoing.sql");
            }
        }
    }
}
