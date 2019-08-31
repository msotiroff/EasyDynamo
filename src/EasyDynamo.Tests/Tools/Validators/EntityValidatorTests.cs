using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Tests.Fakes;
using EasyDynamo.Tools.Validators;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace EasyDynamo.Tests.Tools.Validators
{
    public class EntityValidatorTests
    {
        private readonly EntityValidator validator;
        private readonly Mock<IEntityConfiguration<FakeEntity>> entityConfigMock;
        private readonly Mock<IEntityConfigurationProvider> entityConfigurationProviderMock;

        public EntityValidatorTests()
        {
            this.entityConfigurationProviderMock = new Mock<IEntityConfigurationProvider>();
            this.entityConfigMock = new Mock<IEntityConfiguration<FakeEntity>>();
            this.validator = new EntityValidator(this.entityConfigurationProviderMock.Object);

            this.entityConfigurationProviderMock
                .Setup(p => p.GetEntityConfiguration(It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns(this.entityConfigMock.Object);
        }

        [Fact]
        public void Validate_NoPrimaryKey_ThrowsException()
        {
            var entity = new FakeEntity();

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(typeof(FakeDynamoContext), entity));
        }

        [Fact]
        public void Validate_HasPrimaryKeyRequiredMemberIsNull_ThrowsException()
        {
            this.entityConfigMock
                .SetupGet(c => c.Properties)
                .Returns(new List<PropertyConfiguration<FakeEntity>>
                {
                    new PropertyConfiguration<FakeEntity>(nameof(FakeEntity.Content))
                    {
                        IsRequired = true
                    }
                });

            var entity = new FakeEntity
            {
                Id = 1
            };

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(typeof(FakeDynamoContext), entity));
        }

        [Fact]
        public void Validate_MemberWithRequiredAttributeIsNull_ThrowsException()
        {
            var entity = new FakeEntity
            {
                Id = 1
            };

            Assert.Throws<EntityValidationFailedException>(
                () => this.validator.Validate(typeof(FakeDynamoContext), entity));
        }
    }
}
