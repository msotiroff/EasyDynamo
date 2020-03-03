using System;
using System.Threading.Tasks;

namespace EasyDynamo.Abstractions
{
    public interface ITableCreator
    {
        Task<string> CreateTableAsync(Type contextType, Type entityType, string tableName);
        
        Task UpdateTableAsync(Type contextType, Type entityType, string tableName);
    }
}