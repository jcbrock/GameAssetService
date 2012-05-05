using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Serialization;

namespace GameService.Common
{
    [Serializable]
    public class UserNotFoundException : Exception, ISerializable
    {
          public UserNotFoundException()
        {
            // Add implementation.
        }
        public UserNotFoundException(string message)
            : base(message)
        {
            // Add implementation.
        }
        public UserNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
            // Add implementation.
        }

        // This constructor is needed for serialization.
        protected UserNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Add implementation.
        }
    }
}