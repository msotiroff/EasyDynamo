using EasyDynamo.Tools.Validators;
using System;

namespace EasyDynamo.Config
{
    public class GlobalSecondaryIndexConfiguration
    {
        private long readCapacityUnits;
        private long writeCapacityUnits;

        public GlobalSecondaryIndexConfiguration()
        {
            this.ReadCapacityUnits = 1;
            this.WriteCapacityUnits = 1;
        }

        public string IndexName { get; set; }

        public string HashKeyMemberName { get; set; }

        public Type HashKeyMemberType { get; set; }

        public string RangeKeyMemberName { get; set; }

        public Type RangeKeyMemberType { get; set; }

        public long ReadCapacityUnits
        {
            get => this.readCapacityUnits;
            set
            {
                InputValidator.ThrowIfNotPositive(value, string.Format(
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
    }
}
