﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Data.Sqlite;

namespace TheUltimateBot.Data
{
    public class SqliteDataConnector : IDataConnector
    {
        public SqliteConnection connection;
        public DiscordGuild server { get; set; }

        public SqliteDataConnector(DiscordGuild server, bool openreadonly = false)
        {
            bool needToCreateTables = false;
            if (!Directory.Exists(".\\" + server.Name))
            {
                Directory.CreateDirectory(".\\" + server.Name);
            }

            if (!File.Exists(".\\" + server.Name + "\\data.db3"))
            {
                needToCreateTables = true;
            }

            SqliteConnection connection;

            if (openreadonly && !needToCreateTables)
            {
                connection = new SqliteConnection("Data Source=.\\" + server.Name + "\\data.db3;Read Only=True;");
            }
            else
            {
                connection = new SqliteConnection("Data Source=.\\" + server.Name + "\\data.db3;");
            }

            connection.Open();

            if (needToCreateTables)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE ACTIVE(ID INTEGER PRIMARY KEY AUTOINCREMENT, USER INT NOT NULL, LASTACTIVE INT NOT NULL)";
                cmd.ExecuteNonQuery();

                cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE INACTIVE(ID INTEGER PRIMARY KEY AUTOINCREMENT, USER INT NOT NULL)";
                cmd.ExecuteNonQuery();
            }

            this.connection = connection;
            this.server = server;
        }

        public List<DiscordMember> GetActive()
        {
            var cmd = connection.CreateCommand();
            List<DiscordMember> members = new List<DiscordMember>();
            cmd.CommandText = "SELECT * FROM ACTIVE";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    members.Add(server.GetMemberAsync((ulong)(reader.GetInt64(1))).Result);
                }
            }
            return members;
        }

        public List<DiscordMember> GetInactive()
        {
            var cmd = connection.CreateCommand();
            List<DiscordMember> members = new List<DiscordMember>();
            cmd.CommandText = "SELECT * FROM INACTIVE";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    members.Add(server.GetMemberAsync((ulong)(reader.GetInt64(1))).Result);
                }
            }
            return members;
        }

        public int GetUnixTimestamp() => (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        public void SetActive(DiscordMember member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO ACTIVE (USER, LASTACTIVE) VALUES (" + member.Id + ", " + GetUnixTimestamp() + ")";
            cmd.ExecuteNonQuery();
        }

        public void SetInactive(DiscordMember member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM ACTIVE WHERE USER=" + member.Id;
            var result = cmd.ExecuteReader();

            if (!result.HasRows)
            {
                throw new KeyNotFoundException(member.Mention + " was not found in active users.");
            }

            cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO ACTIVE (USER, LASTACTIVE) VALUES (" + member.Id + ", " + GetUnixTimestamp() + ")";
            cmd.ExecuteNonQuery();

            cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM ACTIVE WHERE USER=" + member.Id;
            cmd.ExecuteNonQuery();
        }

        public void Remove(DiscordMember member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM ACTIVE WHERE USER=" + member.Id;
            cmd.ExecuteNonQuery();

            cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM INACTIVE WHERE USER=" + member.Id;
            cmd.ExecuteNonQuery();
        }

        public List<DiscordMember> AddRoleAsActive(DiscordGuild guild, DiscordRole role)
        {
            var result = new List<DiscordMember>();

            foreach (var user in guild.GetAllMembersAsync().Result)
            {
                if (user.Roles.Contains(role))
                {
                    SetActive(user);
                    result.Add(user);
                }
            }

            return result;
        }

        public int GetLastActivity(DiscordMember member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT LASTACTIVE FROM ACTIVE WHERE USER=" + member.Id;
            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                return reader.GetInt32(0);
            }

            return -1;
        }

        public void UpdateActivity(DiscordUser author)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE ACTIVE SET LASTACTIVE=" + GetUnixTimestamp() + " WHERE USER=" + author.Id;
            cmd.ExecuteNonQuery();
        }

        public bool IsInDatabase(DiscordUser member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM ACTIVE WHERE USER=" + member.Id;
            bool result;
            using (var reader = cmd.ExecuteReader())
            {
                result = reader.HasRows;
            }
            return result;
        }

        public bool IsInDatabase(DiscordMember member)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM ACTIVE WHERE USER=" + member.Id;
            bool result;
            using (var reader = cmd.ExecuteReader())
            {
                result = reader.HasRows;
            }
            return result;
        }

        ~SqliteDataConnector()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}