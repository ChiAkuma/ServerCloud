using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ServerCloud.Exceptions
{
    [Serializable]
    internal class NoWhereBug : Exception
    {
        public NoWhereBug()
        {
            LogStackTrace();
        }

        public NoWhereBug(string? message, string additionalInfos) : base(message)
        {
            Console.Error.WriteLine(message);
            LogStackTrace();
            Console.Error.WriteLine("Additional Infos: ");
            Console.Error.WriteLine(additionalInfos);
        }

        public NoWhereBug(string? message, Exception? innerException) : base(message, innerException)
        {
            LogStackTrace();
        }

        protected NoWhereBug(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            LogStackTrace();
        }

        private void LogStackTrace()
        {
            StackTrace stackTrace = new StackTrace(true);
            Console.Error.WriteLine("Please send this to the developer: Stack Trace:");
            Console.Error.WriteLine(stackTrace.ToString());
        }
    }
}