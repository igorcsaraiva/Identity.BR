using FluentAssertions;
using Identity.BR.ValueObjects;

namespace Identity.BR.Test
{
    public class CpfTests
    {
        private const string ValidCpfRaw = "12345678909";
        private const string ValidCpfMasked = "123.456.789-09";

        [Theory]
        [InlineData(ValidCpfRaw)]
        [InlineData(ValidCpfMasked)]
        [InlineData("  12345678909  ")] // Espaços
        public void Constructor_WithValidInput_ShouldCreateInstance(string input)
        {
            // Act
            var cpf = new CPF(input);

            // Assert
            cpf.IsValid.Should().BeTrue();
            ((string)cpf).Should().Be(ValidCpfRaw);
        }

        [Theory]
        [InlineData("11111111111")] // Uniforme
        [InlineData("12345678900")] // DV1 inválido
        [InlineData("12345678901")] // DV2 inválido
        [InlineData("123")]          // Curto
        [InlineData("123456789012345")] // Longo
        [InlineData("abc.def.ghi-jk")]  // Letras
        public void Constructor_WithInvalidInput_ShouldThrowArgumentException(string input)
        {
            // Act
            Action act = () => _ = new CPF(input);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Parse_ValidInput_ShouldReturnCPF()
        {
            // Act
            var result = CPF.Parse(ValidCpfMasked);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Parse_NullOrEmpty_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CPF.Parse(null!));
            Assert.Throws<ArgumentNullException>(() => CPF.Parse(string.Empty));
        }

        [Theory]
        [InlineData(ValidCpfRaw, true)]
        [InlineData(ValidCpfMasked, true)]
        [InlineData("00000000000", false)] // Uniforme clássico
        [InlineData("123.456.789-10", false)] // DV Errado
        public void TryParse_ShouldValidateCorrectly(string input, bool expectedSuccess)
        {
            // Act
            bool success = CPF.TryParse(input, null, out var result);

            // Assert
            success.Should().Be(expectedSuccess);
            if (success)
                result.IsValid.Should().BeTrue();
            else
                result.Should().Be(default(CPF));
        }

        [Fact]
        public void ToString_ShouldReturnFormattedCpf()
        {
            // Arrange
            var cpf = new CPF(ValidCpfRaw);

            // Act
            var formatted = cpf.ToString();

            // Assert
            formatted.Should().Be(ValidCpfMasked);
        }

        [Fact]
        public void Equality_SameCpf_ShouldBeEqual()
        {
            // Arrange
            CPF cpf1 = ValidCpfRaw;
            CPF cpf2 = ValidCpfMasked;

            // Assert
            (cpf1 == cpf2).Should().BeTrue();
            cpf1.Equals(cpf2).Should().BeTrue();
            cpf1.GetHashCode().Should().Be(cpf2.GetHashCode());
        }

        [Fact]
        public void CompareTo_ShouldSortCorrectly()
        {
            // Arrange
            // Simulando CPFs válidos para ordenação
            CPF menor = "34385775001"; // Exemplo hipotético
            CPF maior = "92958187098";

            // Assert
            menor.CompareTo(maior).Should().BeNegative();
        }

        [Fact]
        public void ImplicitOperator_NullValue_ShouldReturnEmptyString()
        {
            // Arrange
            CPF cpf = default;

            // Act
            string result = cpf;

            // Assert
            result.Should().BeEmpty();
        }
    }
}
