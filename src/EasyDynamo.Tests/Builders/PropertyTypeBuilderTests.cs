using EasyDynamo.Tests.Fakes;
using Xunit;

namespace EasyDynamo.Tests.Builders
{
    public class PropertyTypeBuilderTests
    {
        private const string SampleDefaultValue = "Some content";
        private readonly string propertyName;
        private readonly PropertyConfigurationFake propertyConfig;
        private readonly PropertyTypeBuilderFake builder;

        public PropertyTypeBuilderTests()
        {
            this.propertyName = nameof(FakeEntity.Content);
            this.propertyConfig = new PropertyConfigurationFake(this.propertyName);
            this.builder = new PropertyTypeBuilderFake(this.propertyConfig);
        }
        
        [Fact]
        public void HasDefaultValue_SetCorrectValue()
        {
            this.builder.HasDefaultValue(SampleDefaultValue);

            Assert.Equal(SampleDefaultValue, this.propertyConfig.DefaultValue);
        }

        [Fact]
        public void HasDefaultValue_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.HasDefaultValue(SampleDefaultValue);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsRequired_SetCorrectValue(bool value)
        {
            this.builder.IsRequired(value);

            Assert.Equal(value, this.propertyConfig.IsRequired);
        }

        [Fact]
        public void IsRequired_DefaultToTrue()
        {
            this.builder.IsRequired();

            Assert.True(this.propertyConfig.IsRequired);
        }

        [Fact]
        public void IsRequired_ReturnsSameInstanceOfBuilder()
        {
            var returnedBuilder = this.builder.IsRequired(true);

            Assert.Equal(this.builder, returnedBuilder);
        }
    }
}
