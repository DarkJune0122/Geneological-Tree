using Gen.Networking;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Gen.Data
{
    public sealed class MongoDB : IDatabaseProcessor
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "POST" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public async Task<CommunicationResult<bool>> Register(LoginInfo info)
        {
            // Requesting:
            string payload = JsonUtility.ToJson(new LoginData(info.Username, info.Password));
            using UnityWebRequest request = UnityWebRequest.Post(
                IDatabaseProcessor.ServerURI + "register",
                payload,
                "application/json");
            request.timeout = IDatabaseProcessor.DefaultTimeout;

            // Handling:
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await WaitForOperationEnd(operation);
            IDatabaseProcessor.DebugMessage = request.downloadHandler.text;
            return new CommunicationResult<bool>(
                request.result == UnityWebRequest.Result.Success,
                (int)request.responseCode,
                request.responseCode == 201L,
                request.error);
        }
        public async Task<CommunicationResult<(bool, string)>> Login(LoginInfo info)
        {
            // Requesting:
            string payload = JsonUtility.ToJson(new LoginData(info.Username, info.Password));
            using UnityWebRequest request = UnityWebRequest.Post(
                IDatabaseProcessor.ServerURI + "login",
                payload,
                "application/json");
            request.timeout = IDatabaseProcessor.DefaultTimeout;

            // Handling:
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await WaitForOperationEnd(operation);
            (bool valid, string data) result = new(request.responseCode == 201L, string.Empty);
            if (result.valid) result.data = request.downloadHandler.text;
            return new CommunicationResult<(bool, string)>(
                request.result == UnityWebRequest.Result.Success,
                (int)request.responseCode,
                result,
                request.error);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "PUT" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public async Task<CommunicationResult<bool>> Delete(string userId)
        {
            // Requesting:
            string payload = JsonUtility.ToJson(new EntryIndex(userId));
            UnityWebRequest request = UnityWebRequest.Put(
                IDatabaseProcessor.ServerURI + "remdoc",
                payload);
            request.timeout = IDatabaseProcessor.DefaultTimeout;
            request.SetRequestHeader("Content-Type", "application/json");

            // Handling:
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await WaitForOperationEnd(operation);

            CommunicationResult<bool> result = new(
                request.result == UnityWebRequest.Result.Success,
                (int)request.responseCode,
                request.responseCode == 201L,
                request.error);

            // Finalizing:
            request.Dispose();
            return result;
        }
        public async Task<CommunicationResult<bool>> Put(string userId, string document)
        {
            // Requesting:
            string payload = JsonUtility.ToJson(new DocumentEntry(userId, document));
            UnityWebRequest request = UnityWebRequest.Put(
                IDatabaseProcessor.ServerURI + "setdoc",
                payload);
            request.timeout = IDatabaseProcessor.DefaultTimeout;
            request.SetRequestHeader("Content-Type", "application/json");

            // Handling:
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await WaitForOperationEnd(operation);
            CommunicationResult<bool> result = new(
                request.result == UnityWebRequest.Result.Success,
                (int)request.responseCode,
                request.responseCode == 201L,
                request.error);

            // Finalizing:
            request.Dispose();
            return result;
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "GET" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public async Task<CommunicationResult<string>> Get(string userId)
        {
            // Requesting:
            UnityWebRequest request = UnityWebRequest.Get(IDatabaseProcessor.ServerURI + "getdoc?id=" + userId);
            request.timeout = IDatabaseProcessor.DefaultTimeout;

            // Handling:
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await WaitForOperationEnd(operation);
            CommunicationResult<string> result = new(
                request.result == UnityWebRequest.Result.Success,
                (int)request.responseCode,
                request.downloadHandler.text,
                request.error);

            // Finalizing:
            request.Dispose();
            return result;
        }



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private async Task WaitForOperationEnd(UnityWebRequestAsyncOperation operation)
        {
            do
            {
                await Task.Delay(50);
            }
            while (!operation.isDone);
        }

        [Serializable]
        private struct EntryIndex
        {
            public string userId;

            public EntryIndex(string userId)
            {
                this.userId = userId;
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
        }
    }
}
