using CRUD;
using System.Threading.Tasks;
using UnityEngine;

namespace Gen.Networking
{
    public interface IDatabaseProcessor
    {
        public static int DefaultTimeout => UseLocalServer ? 15 : 30;
        public static string ServerURI => UseLocalServer
            ? "http://localhost:52114/"
            : "https://gen-tree-server.vercel.app/";

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static bool UseLocalServer
        {
            get => PlayerPrefs.GetInt("use-local-server") != 0;
            set
            {
                if (PlayerPrefs.GetInt("use-local-server") != 0 == value) return;
                PlayerPrefs.SetInt("use-local-server", value ? 1 : 0);
            }
        }
        public static bool UseDebugServer
        {
            get => PlayerPrefs.GetInt("use-debug-server") != 0;
            set
            {
                if (PlayerPrefs.GetInt("use-debug-server") != 0 == value) return;
                PlayerPrefs.SetInt("use-debug-server", value ? 1 : 0);
                DB.SetProcessor(GetCurrent());
            }
        }

        // Debug:
        public static string DebugMessage { get; set; }
        public static int DebugIncrement { get; set; }

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "POST" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        Task<CommunicationResult<bool>> Register(LoginInfo info);
        Task<CommunicationResult<(bool, string)>> Login(LoginInfo info);

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "PUT" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        Task<CommunicationResult<bool>> Put(string userId, string document);
        Task<CommunicationResult<bool>> Delete(string userId);

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     "GET" Requests
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        Task<CommunicationResult<string>> Get(string userId);

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static IDatabaseProcessor GetCurrent()
        {
            return PlayerPrefs.GetInt("use-debug-server") != 0 ? new Data.DebugDB() : new Data.MongoDB();
        }
    }
}
