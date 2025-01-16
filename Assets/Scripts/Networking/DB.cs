using Gen;
using Gen.Networking;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Tree = Gen.Data.Tree;

namespace CRUD
{
    public static class DB
    {
        private static bool isInitialized = false;

        // Local:
        private static IDatabaseProcessor processor;

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Initialization
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;
            SetProcessor(IDatabaseProcessor.GetCurrent());
        }

        public static void SetProcessor(IDatabaseProcessor processor)
        {
            if (processor == null) return;
            DB.processor = processor;
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Web Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static async Task<CommunicationResult<bool>> Register(LoginInfo info)
        {
            Init();

            try
            {
                return await processor.Register(info);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = ex.Message + '\n' + ex.StackTrace;
                return new CommunicationResult<bool>(false, 400, false, ex.Message);
            }
        }

        public static async Task<CommunicationResult<(bool, string)>> Login(LoginInfo info)
        {
            Init();

            try
            {
                return await processor.Login(info);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = ex.Message + '\n' + ex.StackTrace;
                return new CommunicationResult<(bool, string)>(false, 400, new(false, string.Empty), ex.Message);
            }
        }

        public static async Task<CommunicationResult<bool>> Put(string userId, Tree document)
        {
            Init();
            if (LoginInfo.Active == null)
            {
                Debug.LogWarning("Unauthenticated access error.");
                return await Task.FromResult(new CommunicationResult<bool>(false, 401, false, "Not authenticated."));
            }

            try
            {
                string raw = JsonUtility.ToJson(document);
                string data = LoginInfo.Active.Encrypt(raw);
                return await processor.Put(userId, data);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = "" + document + '\n' + ex.Message + '\n' + ex.StackTrace;
                return new CommunicationResult<bool>(false, 400, false, ex.Message);
            }
        }

        public static async Task<CommunicationResult<Tree>> Get(string userId)
        {
            Init();
            IDatabaseProcessor.DebugMessage = string.Empty;
            if (LoginInfo.Active == null)
            {
                Manager.LastCommunicationResult = new(false, "Unauthenticated access error.");
                return await Task.FromResult(new CommunicationResult<Tree>(false, 401, default, "Not authenticated."));
            }

            CommunicationResult<string> comm;
            try
            {
                comm = await processor.Get(userId);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage += ex.Message + '\n' + ex.StackTrace + '\n';
                return new CommunicationResult<Tree>(false, 400, default, ex.Message);
            }

            if (!comm.succeeded)
            {
                return await Task.FromResult(new CommunicationResult<Tree>(false, comm.status, default, comm.message));
            }

            if (string.IsNullOrWhiteSpace(comm.result))
            {
                return await Task.FromResult(new CommunicationResult<Tree>(true, comm.status, new Tree(), comm.message));
            }

            Tree document;
            try
            {
                string raw = LoginInfo.Active.Decrypt(comm.result);
                document = JsonUtility.FromJson<Tree>(raw);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = "Raw: " + comm.result + '\n' + ex.Message + '\n' + ex.StackTrace;
                return new CommunicationResult<Tree>(true, 400, new Tree(), ex.Message);
            }

            return await Task.FromResult(new CommunicationResult<Tree>(true, comm.status, document, comm.message));
        }

        public static async Task<CommunicationResult<bool>> Delete(string userId)
        {
            Init();
            if (LoginInfo.Active == null)
            {
                Debug.LogWarning("Unauthenticated access error.");
                return await Task.FromResult(new CommunicationResult<bool>(false, 401, false, "Not authenticated."));
            }

            try
            {
                return await processor.Delete(userId);
            }
            catch (Exception ex)
            {
                IDatabaseProcessor.DebugMessage = ex.Message + '\n' + ex.StackTrace;
                return new CommunicationResult<bool>(false, 400, default, ex.Message);
            }
        }
    }
}
