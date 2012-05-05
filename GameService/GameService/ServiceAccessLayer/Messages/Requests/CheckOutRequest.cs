using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameService.ServiceAccessLayer.Messages.Requests
{
    public class CheckOutRequest : RequestBase
    {
       // public int UserId { get; set; }
       // public string FilePath { get; set; } //lookup - how do I do the file vs folder thing again?
       // public bool IsDirectory { get; set; }

        //It is a property on the File object - comes from the client upon insert.
    }
}