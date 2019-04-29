using System;

namespace EasyDynamo.Config
{
    internal class PropertyConfiguration<TEntity>
    {
        internal PropertyConfiguration(string memberName)
        {
            this.MemberName = memberName;
            this.PropertyType = typeof(TEntity).GetProperty(memberName).PropertyType;
        }
        
        public string MemberName { get; }
        
        public Type PropertyType { get; }

        public object DefaultValue { get; set; }

        public bool IsRequired { get; set; }
    }
}
