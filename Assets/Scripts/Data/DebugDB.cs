using Gen.Networking;
using OptiLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gen.Data
{
    public sealed class DebugDB : IDatabaseProcessor
    {
        public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"My Games/{Application.companyName}/{Application.productName}");
        public static readonly string DatabaseFileName = "User Database.json";

        /// <summary>
        /// Our entire database. (LOL) (Excluding tree files)
        /// </summary>
        private UserDatabase userDatabase;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "POST" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public async Task<CommunicationResult<bool>> Register(LoginInfo info)
        {
            userDatabase ??= await ReadDatabase(DatabaseFileName);
            IDatabaseProcessor.DebugMessage = userDatabase.raw.StringEnumerable("User Database:");

            // Saves credentials:
            if (userDatabase.ContainsKey(info.Username))
            {
                return new CommunicationResult<bool>(true, 400, false, "Username is already taken");
            }
            else userDatabase.Add(new LoginData(info.Username.ToLower(), info.Password));
            await SaveDatabase(userDatabase, DatabaseFileName);
            return new CommunicationResult<bool>(true, 201, true, "User registered!");
        }
        public async Task<CommunicationResult<(bool, string)>> Login(LoginInfo info)
        {
            userDatabase ??= await ReadDatabase(DatabaseFileName);
            IDatabaseProcessor.DebugMessage = userDatabase.raw.StringEnumerable("User Database:");

            // Signing in the system:
            if (!userDatabase.TryGetValue(info.Username, out LoginData data))
            {
                return new CommunicationResult<(bool, string)>(true, 404, new(false, string.Empty), "User not found");
            }

            if (!data.password.Equals(info.Password))
            {
                return new CommunicationResult<(bool, string)>(true, 401, new(false, string.Empty), "Invalid password");
            }

            string userId = GetUserID(info.Username);
            await SaveDatabase(userDatabase, DatabaseFileName);
            return new CommunicationResult<(bool, string)>(true, 201, new(true, userId));
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "PUT" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public Task<CommunicationResult<bool>> Delete(string userId)
        {
            string path = Path.Combine(RootPath, $"Trees/{userId}.gendb");
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.FromResult(new CommunicationResult<bool>(true, 201, true, "Document Deleted"));
        }
        public async Task<CommunicationResult<bool>> Put(string userId, string document)
        {
            string path = Path.Combine(RootPath, $"Trees/{userId}.gendb");
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(path, document);
            return new CommunicationResult<bool>(true, 201, true, "Document Set");
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "GET" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public async Task<CommunicationResult<string>> Get(string userId)
        {
            string path = Path.Combine(RootPath, $"trees/{userId}.gendb");
            string content = string.Empty;
            if (File.Exists(path))
            {
                content = await File.ReadAllTextAsync(path);
            }

            return new CommunicationResult<string>(true, 201, content);
        }



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private string GetUserID(string username)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{username}"));

            // Convert the hash to a hexadecimal string
            StringBuilder hex = new(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private async Task SaveDatabase(UserDatabase database, string relativePath)
        {
            if (database == null || database.Count == 0) return;
            try
            {
                string path = Path.Combine(RootPath, relativePath);
                if (!Directory.Exists(RootPath))
                {
                    Directory.CreateDirectory(RootPath);
                }

                await File.WriteAllTextAsync(path, JsonUtility.ToJson(database));
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = ex.Message + '\n' + ex.StackTrace;
            }
        }

        private async Task<UserDatabase> ReadDatabase(string relativePath)
        {
            try
            {
                string path = Path.Combine(RootPath, relativePath);
                if (File.Exists(path))
                {
                    string content = await File.ReadAllTextAsync(path);
                    if (!string.IsNullOrWhiteSpace(content))
                        return JsonUtility.FromJson<UserDatabase>(content);
                }
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = ex.Message + '\n' + ex.StackTrace;
            }

            return new UserDatabase();
        }

        [Serializable]
        private sealed class UserDatabase
        {
            public List<LoginData> raw = new();

            // Public Properties:
            public int Count => raw.Count;

            // Public Methods:
            public bool ContainsKey(string key)
            {
                return raw.FindIndex((d) => d.username.Equals(key, StringComparison.OrdinalIgnoreCase)) != -1;
            }

            public bool TryGetValue(string key, out LoginData data)
            {
                int index = raw.FindIndex((d) => d.username.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (index == -1)
                {
                    data = default;
                    return false;
                }

                data = raw[index];
                return true;
            }

            public void Add(LoginData data) => Add(data.username, data);
            public void Add(string key, LoginData data)
            {
                int index = raw.FindIndex((d) => d.username.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (index != -1) raw[index] = data;
                else
                {
                    raw.Add(data);
                }
            }

        }

        [Serializable]
        private struct DocumentEntry
        {
            public string userId;
            public string document;

            public DocumentEntry(string userId, string document)
            {
                this.userId = userId;
                this.document = document;
            }

            public override readonly string ToString()
            {
                return $"{userId} : {document}";
            }
        }

        [Serializable]
        private struct LoginData
        {
            public string username;
            public string password;

            public LoginData(string username, string password)
            {
                this.username = username;
                this.password = password;
            }

            public override readonly string ToString()
            {
                return $"{username} : {password}";
            }
        }
    }
}
