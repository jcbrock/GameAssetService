using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Serialization;

namespace GameService.Common
{
    [Serializable]
    public class DatabaseRecordNotFoundException : Exception, ISerializable
    {
        public DatabaseRecordNotFoundException()
        {
            // Add implementation.
        }
        public DatabaseRecordNotFoundException(string message)
            : base(message)
        {
            // Add implementation.
        }
        public DatabaseRecordNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
            // Add implementation.
        }

        // This constructor is needed for serialization.
        protected DatabaseRecordNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Add implementation.
        }
    }
}