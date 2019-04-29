using System;
using System.Threading.Tasks;

namespace EasyDynamo.Abstractions
{
    public interface ITableDropper
    {
        Task DropTableAsync(string tableName);
    }
}