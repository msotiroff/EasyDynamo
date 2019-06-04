using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Tests.Fakes;
using EasyDynamo.Tools.Validators;
using Xunit;

namespace EasyDynamo.Tests.Tools.Validators
{
    public class EntityValidatorTests
    {
        private readonly EntityValidator<FakeEntity> validator;
        private readonly EntityConfiguration<FakeEntity> entityConfig;

        public EntityValidatorTests()
        {
            this.entityConfig = new EntityConfigurationFake().BaseInstance;
            this.validator = new EntityValidator<FakeEntity>();
            this.entityConfig.HashKeyMemberExpression = e => e.Id;
            this.entityConfig.HashKeyMemberType = typeof(int);
        }

        [Fact]
        public void Validate_NoPrimaryKey_ThrowsException()
        {
            var entity = new FakeEntity();

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(entity));
        }

        [Fact]
        public void Validate_HasPrimaryKeyRequiredMemberIsNull_ThrowsException()
        {
            this.entityConfig.Properties.Add(
                new PropertyConfiguration<FakeEntity>(nameof(FakeEntity.Content))
                {
                    IsRequired = true
                });

            var entity = new FakeEntity
            {
                Id = 1
            };

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(entity));
        }

        [Fact]
        public void Validate_MemberWithRequiredAttributeIsNull_ThrowsException()
        {
            var entity = new FakeEntity
            {
                Id = 1
            };

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(entity));
        }
    }
}
