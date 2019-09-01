using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Tests.Fakes;
using Moq;
using System;
using Xunit;

namespace EasyDynamo.Tests.Builders
{
    public class ModelBuilderTests
    {
        private readonly Mock<IDynamoContextOptions> optionsMock;
        private readonly ModelBuilderFake builder;

        public ModelBuilderTests()
        {
            this.optionsMock = new Mock<IDynamoContextOptions>();
            this.builder = new ModelBuilderFake(optionsMock.Object);
        }

        [Fact]
        public void ApplyConfiguration_ConfigurationIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.ApplyConfiguration(default(EntityTypeConfugurationFake)));
        }

        [Fact]
        public void ApplyConfiguration_ValidConfiguration_InvokeConfigure()
        {
            var configuration = new EntityTypeConfugurationFake();

            this.builder.ApplyConfiguration(configuration);

            Assert.True(configuration.ConfigureInvoked);
        }

        [Fact]
        public void ApplyConfiguration_ValidConfiguration_InvokeConfigureWithCorrectType()
        {
            var configuration = new EntityTypeConfugurationFake();

            this.builder.ApplyConfiguration(configuration);

            Assert.Equal(
                typeof(EntityTypeBuilder<FakeDynamoContext, FakeEntity>), 
                configuration.ConfigureInvokedWithBuilderType);
        }

        [Fact]
        public void ApplyConfiguration_ValidConfiguration_ReturnsSameInstanceOfBuilder()
        {
            var configuration = new EntityTypeConfugurationFake();

            var returnedBuilder = this.builder.ApplyConfiguration(configuration);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void Entity_ReturnsEntityBuilderOfCorrectType()
        {
            var entBuilder = this.builder.Entity<FakeDynamoContext, FakeEntity>();

            Assert.Equal(
                typeof(EntityTypeBuilder<FakeDynamoContext, FakeEntity>), 
                entBuilder.GetType());
        }

        [Fact]
        public void Entity_AddsCorrectKeyValuePairInTheMap()
        {
            this.builder.Entity<FakeDynamoContext, FakeEntity>();

            Assert.Single(this.builder.EntityConfigurationsFromBase);
            Assert.Contains(
                this.builder.EntityConfigurationsFromBase,
                kvp => kvp.Key == typeof(FakeEntity));
            Assert.Contains(
                this.builder.EntityConfigurationsFromBase,
                kvp => kvp.Value.GetType() == typeof(EntityConfiguration<FakeDynamoContext, FakeEntity>));
        }

        [Fact]
        public void Entity_BuildActionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.Entity(default(Action<IEntityTypeBuilder<FakeDynamoContext, FakeEntity>>)));
        }

        [Fact]
        public void Entity_ValidBuildAction_InvokeAction()
        {
            Action<IEntityTypeBuilder<FakeDynamoContext, FakeEntity>> buildAction = e => 
            {
                e.HasPrimaryKey(ent => ent.Id);
            };

            this.builder.Entity<FakeDynamoContext, FakeEntity>(buildAction);

            Assert.Single(buildAction.GetInvocationList());
        }

        [Fact]
        public void Entity_ValidBuildAction_ReturnsSameInstanceOfBuilder()
        {
            Action<IEntityTypeBuilder<FakeDynamoContext, FakeEntity>> buildAction = e =>
            {
                e.HasPrimaryKey(ent => ent.Id);
            };

            var returnedBuilder = this.builder.Entity<FakeDynamoContext, FakeEntity>(buildAction);

            Assert.Equal(this.builder, returnedBuilder);
        }
    }
}
