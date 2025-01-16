using System;

namespace Gen.Networking
{
    [Serializable]
    public readonly struct CommunicationResult<TResult>
    {
        public readonly bool succeeded;
        public readonly int status;
        public readonly TResult result;
        public readonly string message;

        public CommunicationResult(bool succeeded, int status, TResult result, string message = "")
        {
            this.succeeded = succeeded;
            this.status = status;
            this.result = result;
            this.message = message;
        }

        public override string ToString()
        {
            return $"{succeeded}  -  {status}  -  {result}  -  {message}";
        }
    }
}
