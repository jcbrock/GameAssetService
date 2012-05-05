using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameService.ServiceAccessLayer.Messages.Responses
{
    public class ResponseBase
    {
        private bool _success;

        public bool Success
        {   
            get { return _success; }
            set { _success = value; }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}