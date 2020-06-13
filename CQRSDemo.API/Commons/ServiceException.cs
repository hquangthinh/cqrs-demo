using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;

namespace CQRSDemo.API.Commons
{
    public class ServiceException : Exception
    {
        public ServiceException() : base() { }
        public ServiceException(string message) : base(message) { }
        [SecuritySafeCritical]
        protected ServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public ServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
