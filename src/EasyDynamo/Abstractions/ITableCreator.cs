using System;
using System.Threading.Tasks;

namespace EasyDynamo.Abstractions
{
    public interface ITableCreator
    {
        Task<string> CreateTableAsync(Type entityType, string tableName);
    }
}