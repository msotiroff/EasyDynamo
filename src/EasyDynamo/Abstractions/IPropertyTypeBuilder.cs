using System;

namespace EasyDynamo.Abstractions
{
    public interface IPropertyTypeBuilder
    {
        IPropertyTypeBuilder HasDefaultValue(object defaultValue);

        IPropertyTypeBuilder IsRequired(bool required = true);
    }
}
