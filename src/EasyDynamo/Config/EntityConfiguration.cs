using EasyDynamo.Abstractions;
using EasyDynamo.Tools.Validators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EasyDynamo.Config
{
    public class EntityConfiguration<TEntity> : IEntityConfiguration where TEntity : class
    {
        private long readCapacityUnits;
        private long writeCapacityUnits;

        private static volatile EntityConfiguration<TEntity> instance;
        private static readonly object instanceLoker = new object();

        private EntityConfiguration()
        {
            this.ValidateOnSave = true;
            this.Properties = new List<PropertyConfiguration<TEntity>>();
            this.Indexes = new HashSet<GlobalSecondaryIndexConfiguration>();
            this.IgnoredMembersNames = new HashSet<string>();
            this.IgnoredMembersExpressions = new HashSet<Expression<Func<TEntity, object>>>();
            this.ReadCapacityUnits = 1;
            this.WriteCapacityUnits = 1;
        }

        internal static EntityConfiguration<TEntity> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLoker)
                    {
                        if (instance == null)
                        {
                            instance = new EntityConfiguration<TEntity>();
                        }
                    }
                }

                return instance;
            }
        }

        internal ICollection<PropertyConfiguration<TEntity>> Properties { get; }

        public string TableName { get; set; }

        public string HashKeyMemberName { get; set; }

        public long ReadCapacityUnits
        {
            get => this.readCapacityUnits;
            set
            {
                InputValidator.ThrowIfNotPositive(value,string.Format(
                    ExceptionMessage.PositiveIntegerNeeded, nameof(this.ReadCapacityUnits)));

                this.readCapacityUnits = value;
            }
        }

        public long WriteCapacityUnits
        {
            get => this.writeCapacityUnits;
            set
            {
                InputValidator.ThrowIfNotPositive(value, string.Format(
                    ExceptionMessage.PositiveIntegerNeeded, nameof(this.WriteCapacityUnits)));

                this.writeCapacityUnits = value;
            }
        }

        public Type HashKeyMemberType { get; set; }

        public bool ValidateOnSave { get; set; }
        
        public Expression<Func<TEntity, object>> HashKeyMemberExpression { get; set; }

        public ISet<GlobalSecondaryIndexConfiguration> Indexes { get; }

        public ISet<string> IgnoredMembersNames { get; }

        public ISet<Expression<Func<TEntity, object>>> IgnoredMembersExpressions { get; }
    }
}
