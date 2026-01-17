using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.DataAccess.Repository.IRepository
{
    public interface ISP_CALL:IDisposable
    {
        void Execute(string procedureName, DynamicParameters param = null); //For Create , Update , Delete
        T Single<T>(string procedureName, DynamicParameters param = null); //Find Code
        T OneRecord<T>(string procedureName, DynamicParameters param = null); //For Aggregate Methods(MAX, MIN, SUM, COUNT)
        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null); //For Display
        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null);
    }
}
