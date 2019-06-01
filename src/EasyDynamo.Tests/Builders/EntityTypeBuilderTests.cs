using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Tests.Fakes;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace EasyDynamo.Tests.Builders
{
    public class EntityTypeBuilderTests
    {
        private const string SampleIndexName = "GSI_Articles_Title_LastModified";
        
        private readonly Mock<IDynamoContextOptions> contextOptionsMock;
        private readonly EntityConfiguration<FakeEntity> entityConfig;
        private readonly EntityTypeBuilderFake builder;

        public EntityTypeBuilderTests()
        {
            this.contextOptionsMock = new Mock<IDynamoContextOptions>();
            this.entityConfig = new EntityConfigurationFake();
            this.builder = new EntityTypeBuilderFake(
                this.entityConfig, this.contextOptionsMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void HasTable_EmptyTableNAmePassed_ThrowsException(string emptyTableName)
        {
            Assert.Throws<ArgumentNullException>(() => this.builder.HasTable(emptyTableName));
        }

        [Fact]
        public void HasTable_ValidInput_ReturnsSameInstance()
        {
            var returnedBuilder = this.builder.HasTable("table");

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData("Blogs")]
        [InlineData("Articles")]
        [InlineData("Replies")]
        public void HasTable_ValidInput_CallsOptionsMethodWithCorrectArgument(string expected)
        {
            this.builder.HasTable(expected);

            this.contextOptionsMock
                .Verify(opt => opt.UseTableName<FakeEntity>(expected));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void HasGlobalSecondaryIndex_EmptyIndexName_ThrowsException(string emptyTableName)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(
                    emptyTableName,
                    e => e.Title,
                    e => e.LastModified));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_NullHashKeyExpression_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(
                    SampleIndexName,
                    null,
                    e => e.LastModified));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_NullRangeKeyExpression_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(
                    SampleIndexName,
                    e => e.Title,
                    null));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetIndexNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(this.entityConfig.Indexes, i => i.IndexName == SampleIndexName);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetHashKeyNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes, 
                i => i.HashKeyMemberName == nameof(FakeEntity.Title));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetHashKeyTypeCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.HashKeyMemberType == typeof(string));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetRangeKeyNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.RangeKeyMemberName == nameof(FakeEntity.LastModified));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetRangeKeyTypeCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.RangeKeyMemberType == typeof(DateTime));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetReadCapacityUnitsCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                10,
                5);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.ReadCapacityUnits == 10);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidInput_SetWriteCapacityUnitsCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(
                SampleIndexName,
                e => e.Title,
                e => e.LastModified,
                5,
                10);

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.WriteCapacityUnits == 10);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ActionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(
                    default(Action<GlobalSecondaryIndexConfiguration>)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void HasGlobalSecondaryIndex_ActionWithEmptyIndexName_ThrowsException(string empty)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(i =>
                {
                    i.IndexName = empty;
                    i.HashKeyMemberName = nameof(FakeEntity.Title);
                    i.HashKeyMemberType = typeof(string);
                    i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                    i.RangeKeyMemberType = typeof(DateTime);
                    i.ReadCapacityUnits = 3;
                    i.WriteCapacityUnits = 3;
                }));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void HasGlobalSecondaryIndex_ActionWithEmptyHashKeyName_ThrowsException(string empty)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasGlobalSecondaryIndex(i =>
                {
                    i.IndexName = SampleIndexName;
                    i.HashKeyMemberName = empty;
                    i.HashKeyMemberType = typeof(string);
                    i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                    i.RangeKeyMemberType = typeof(DateTime);
                    i.ReadCapacityUnits = 3;
                    i.WriteCapacityUnits = 3;
                }));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetIndexNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.IndexName == SampleIndexName);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetHashKeyNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.HashKeyMemberName == nameof(FakeEntity.Title));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetHashKeyTypeCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.HashKeyMemberType == typeof(string));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetRangeKeyNameCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.RangeKeyMemberName == nameof(FakeEntity.LastModified));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetRangeKeyTypeCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.RangeKeyMemberType == typeof(DateTime));
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetReadCapacityUnitsCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 6;
                i.WriteCapacityUnits = 3;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.ReadCapacityUnits == 6);
        }

        [Fact]
        public void HasGlobalSecondaryIndex_ValidAction_SetWriteCapacityUnitsCorrectly()
        {
            this.builder.HasGlobalSecondaryIndex(i =>
            {
                i.IndexName = SampleIndexName;
                i.HashKeyMemberName = nameof(FakeEntity.Title);
                i.HashKeyMemberType = typeof(string);
                i.RangeKeyMemberName = nameof(FakeEntity.LastModified);
                i.RangeKeyMemberType = typeof(DateTime);
                i.ReadCapacityUnits = 3;
                i.WriteCapacityUnits = 6;
            });

            Assert.Single(this.entityConfig.Indexes);
            Assert.Contains(
                this.entityConfig.Indexes,
                i => i.WriteCapacityUnits == 6);
        }

        [Fact]
        public void HasPrimaryKey_KeyExpressionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.HasPrimaryKey(null));
        }

        [Fact]
        public void HasPrimaryKey_ValidExpression_SetExpressionCorrectly()
        {
            Expression<Func<FakeEntity, object>> expected = e => e.Id;

            this.builder.HasPrimaryKey(expected);

            Assert.Equal(expected, this.entityConfig.HashKeyMemberExpression);
        }

        [Fact]
        public void HasPrimaryKey_ValidExpression_SetPrimaryKeyNameCorrectly()
        {
            this.builder.HasPrimaryKey(e => e.Id);

            Assert.Equal(nameof(FakeEntity.Id), this.entityConfig.HashKeyMemberName);
        }

        [Fact]
        public void HasPrimaryKey_ValidExpression_SetPrimaryKeyTypeCorrectly()
        {
            this.builder.HasPrimaryKey(e => e.Id);

            Assert.Equal(typeof(int), this.entityConfig.HashKeyMemberType);
        }

        [Fact]
        public void HasPrimaryKey_ValidExpression_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasPrimaryKey(e => e.Id);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void Ignore_PropertyExpressionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.Ignore(default(Expression<Func<FakeEntity, object>>)));
        }

        [Fact]
        public void Ignore_ValidPropertyExpression_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.Ignore(e => e.IgnoreMe);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void Ignore_ValidPropertyExpression_SetMemberNameCorrectly()
        {
            this.builder.Ignore(e => e.IgnoreMe);

            Assert.Single(this.entityConfig.IgnoredMembersNames);
            Assert.Contains(
                this.entityConfig.IgnoredMembersNames, 
                im => im == nameof(FakeEntity.IgnoreMe));
        }

        [Fact]
        public void Ignore_ValidPropertyExpression_SetMemberExpressionCorrectly()
        {
            Expression<Func<FakeEntity, object>> expected = e => e.IgnoreMe;

            this.builder.Ignore(expected);

            Assert.Single(this.entityConfig.IgnoredMembersNames);
            Assert.Contains(
                this.entityConfig.IgnoredMembersExpressions,
                expr => expr == expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void Ignore_EmptyMemberName_ThrowsException(string empty)
        {
            Assert.Throws<ArgumentNullException>(() => this.builder.Ignore(empty));
        }

        [Fact]
        public void Ignore_ValidMemberNamePassed_SetNameCorrectly()
        {
            this.builder.Ignore(nameof(FakeEntity.IgnoreMe));

            Assert.Single(this.entityConfig.IgnoredMembersNames);
            Assert.Contains(
                this.entityConfig.IgnoredMembersNames, 
                im => im == nameof(FakeEntity.IgnoreMe));
        }

        [Fact]
        public void Ignore_ValidMemberNamePassed_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.Ignore(nameof(FakeEntity.IgnoreMe));

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void ValidateOnSave_NoParameterPassed_DefaultToTrue()
        {
            this.builder.ValidateOnSave();

            Assert.True(this.entityConfig.ValidateOnSave);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOnSave_ParameterPassed_SetCorrectly(bool expected)
        {
            this.builder.ValidateOnSave(expected);

            Assert.Equal(expected, this.entityConfig.ValidateOnSave);
        }

        [Fact]
        public void ValidateOnSave_ParameterPassed_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.ValidateOnSave(false);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void Property_ExpressionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.Property<string>(null));
        }

        [Fact]
        public void Property_ValidExpression_ReturnsPropertyBuiderOfCorrectType()
        {
            var propBuilder = this.builder.Property(e => e.Content);

            Assert.Equal(typeof(PropertyTypeBuilder<FakeEntity>), propBuilder.GetType());
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void HasReadCapacityUnits_NotPositiveValuePassed_ThrowsException(long value)
        {
            Assert.Throws<ArgumentException>(() => this.builder.HasReadCapacityUnits(value));
        }

        [Fact]
        public void HasReadCapacityUnits_PositiveValuePassed_SetCorrectlyInConfig()
        {
            const long expected = 10;

            this.builder.HasReadCapacityUnits(expected);

            Assert.Equal(expected, this.entityConfig.ReadCapacityUnits);
        }

        [Fact]
        public void HasReadCapacityUnits_PositiveValuePassed_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasReadCapacityUnits(5);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void HasWriteCapacityUnits_NotPositiveValuePassed_ThrowsException(long value)
        {
            Assert.Throws<ArgumentException>(() => this.builder.HasWriteCapacityUnits(value));
        }

        [Fact]
        public void HasWriteCapacityUnits_PositiveValuePassed_SetCorrectlyInConfig()
        {
            const long expected = 10;

            this.builder.HasWriteCapacityUnits(expected);

            Assert.Equal(expected, this.entityConfig.WriteCapacityUnits);
        }

        [Fact]
        public void HasWriteCapacityUnits_PositiveValuePassed_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasWriteCapacityUnits(5);

            Assert.Equal(this.builder, returnedBuilder);
        }
    }
}
