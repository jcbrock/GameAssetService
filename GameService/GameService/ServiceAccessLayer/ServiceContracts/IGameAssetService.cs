using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Linq.Mapping;
using GameService.BusinessObjects.CustomClasses;

namespace GameService
{
      [ServiceContract]
    public interface IGameAssetService
    {
        //todo - Do I need this one?
        [OperationContract]
        bool? IsFileCheckedOut(string fileName);

        [OperationContract]
        List<string> GetLatest(int fileId, int userId);

        [OperationContract]
        CheckInStatus CheckIn(int userId, File file);
        //List<KeyValuePair<int, CheckInStatus>> CheckIn(int userId, File file);

        [OperationContract]
        void Checkout(int userId, int fileId);
    }

    public enum CheckInStatus
    {
        Successful,
        ErrorOccurred,
        FileNotFoundOnServer,
        NewerVersionFoundOnServer,
        FileCheckedOutBySomeoneElse
    }
}
