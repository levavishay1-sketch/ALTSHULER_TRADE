using Alt.DataModel.Crm.Core.Contracts;
using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Guid Create(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        TEntity Get(Guid id, string[] columns);//Retrieve

        TEntity GetFirstOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true);//RetrieveFirstByAttribute

        List<TEntity> GetByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true);//RetrieveMultipleByAttribute

        TEntity GetFirstActivetOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true);//RetrieveFirstActiveByAttribute

        List<TEntity> GetActiveByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true);//RetrieveMultipleActiveByAttribute
    }
}
