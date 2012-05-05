using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Data.Linq.Mapping;

namespace GameService.ServiceAccessLayer.Messages.Requests
{
    [DataContract]
    public abstract class RequestBase
    {        
        private string _connectionString;

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        private Exception _requestException;

        public Exception RequestException
        {
            get { return _requestException; }
            set { _requestException = value; }
        }       
    }

}