using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Tests.Fakes;
using System;
using Xunit;

namespace EasyDynamo.Tests.Builders
{
    public class ModelBuilderTests
    {
        private readonly ModelBuilderFake builder;

        public ModelBuilderTests()
        {
            this.builder = new ModelBuilderFake();
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

            Assert.Equal(typeof(EntityTypeBuilder<FakeEntity>), configuration.ConfigureInvokedWithBuilderType);
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
            var entBuilder = this.builder.Entity<FakeEntity>();

            Assert.Equal(typeof(EntityTypeBuilder<FakeEntity>), entBuilder.GetType());
        }

        [Fact]
        public void Entity_AddsCorrectKeyValuePairInTheMap()
        {
            this.builder.Entity<FakeEntity>();

            Assert.Single(this.builder.EntityConfigurationByEntityTypesFromBase);
            Assert.Contains(
                this.builder.EntityConfigurationByEntityTypesFromBase,
                kvp => kvp.Key == typeof(FakeEntity));
            Assert.Contains(
                this.builder.EntityConfigurationByEntityTypesFromBase,
                kvp => kvp.Value.GetType() == typeof(EntityConfiguration<FakeEntity>));
        }

        [Fact]
        public void Entity_BuildActionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.Entity(default(Action<IEntityTypeBuilder<FakeEntity>>)));
        }

        [Fact]
        public void Entity_ValidBuildAction_InvokeAction()
        {
            Action<IEntityTypeBuilder<FakeEntity>> buildAction = e => 
            {
                e.HasPrimaryKey(ent => ent.Id);
            };

            this.builder.Entity<FakeEntity>(buildAction);

            Assert.Single(buildAction.GetInvocationList());
        }

        [Fact]
        public void Entity_ValidBuildAction_ReturnsSameInstanceOfBuilder()
        {
            Action<IEntityTypeBuilder<FakeEntity>> buildAction = e =>
            {
                e.HasPrimaryKey(ent => ent.Id);
            };

            var returnedBuilder = this.builder.Entity<FakeEntity>(buildAction);

            Assert.Equal(this.builder, returnedBuilder);
        }
    }
}
