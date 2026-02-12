using FluentAssertions;
using Identity.BR.ValueObjects;

namespace Identity.BR.Test
{
    public class CnpjTests
    {
        private const string ValidNumericCnpj = "12345678000195";
        private const string ValidMaskedNumericCnpj = "12.345.678/0001-95";
        private const string ValidAlphanumericCnpj = "SP.MM6.UQL/0001-05"; // Exemplo teórico baseado na regra ASCII-48
        private const string InvalidCnpjUniform = "11111111111111";

        #region Validação e Instalação

        [Theory]
        [InlineData(ValidNumericCnpj)]
        [InlineData(ValidMaskedNumericCnpj)]
        [InlineData(ValidAlphanumericCnpj)] // Alfanumérico válido
        public void Constructor_WithValidInput_ShouldCreateInstance(string input)
        {
            // Act
            var cnpj = new CNPJ(input);

            // Assert
            cnpj.IsValid.Should().BeTrue();
            cnpj._value.Should().HaveLength(14);
        }

        [Theory]
        [InlineData("123")] // Curto
        [InlineData("1234567890123456789")] // Longo
        [InlineData(InvalidCnpjUniform)] // Uniforme
        [InlineData("12.345.678/0001-00")] // DV Inválido
        public void Constructor_WithInvalidInput_ShouldThrowException(string input)
        {
            // Act
            Action act = () => _ = new CNPJ(input);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Parsing (IParsable)

        [Fact]
        public void Parse_ValidInput_ShouldReturnCNPJ()
        {
            // Act
            var result = CNPJ.Parse(ValidMaskedNumericCnpj);

            // Assert
            result.ToString().Should().Be(ValidMaskedNumericCnpj);
        }

        [Fact]
        public void Parse_NullOrEmpty_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CNPJ.Parse(null!));
            Assert.Throws<ArgumentNullException>(() => CNPJ.Parse(string.Empty));
        }

        [Theory]
        [InlineData(ValidNumericCnpj, true)]
        [InlineData(ValidMaskedNumericCnpj, true)]
        [InlineData("00000000000000", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TryParse_ShouldReturnExpectedResult(string? input, bool expectedSuccess)
        {
            // Act
            bool success = CNPJ.TryParse(input, null, out var result);

            // Assert
            success.Should().Be(expectedSuccess);
            if (expectedSuccess)
                result.IsValid.Should().BeTrue();
            else
                result.Should().Be(default(CNPJ));
        }

        #endregion

        #region Comportamento de Formatação

        [Fact]
        public void ToString_ShouldReturnFormattedMask()
        {
            // Arrange
            var cnpj = new CNPJ(ValidNumericCnpj);

            // Act
            var formatted = cnpj.ToString();

            // Assert
            formatted.Should().Be(ValidMaskedNumericCnpj);
        }

        [Fact]
        public void DefaultStruct_ToString_ShouldReturnEmpty()
        {
            // Arrange
            var cnpj = default(CNPJ);

            // Act & Assert
            cnpj.ToString().Should().BeEmpty();
        }

        #endregion

        #region Igualdade e Comparação

        [Fact]
        public void Equality_SameValues_ShouldBeEqual()
        {
            // Arrange
            CNPJ cnpj1 = ValidNumericCnpj;
            CNPJ cnpj2 = ValidMaskedNumericCnpj;

            // Assert
            (cnpj1 == cnpj2).Should().BeTrue();
            cnpj1.Equals(cnpj2).Should().BeTrue();
            cnpj1.GetHashCode().Should().Be(cnpj2.GetHashCode());
        }

        [Fact]
        public void CompareTo_ShouldFollowStringOrdering()
        {
            // Arrange
            CNPJ lower = "11111111000191";
            CNPJ higher = "22222222000191";

            // Assert
            lower.CompareTo(higher).Should().BeNegative();
            higher.CompareTo(lower).Should().BePositive();
        }

        #endregion

        #region Operadores Implícitos

        [Fact]
        public void ImplicitOperator_FromValidString_ShouldWork()
        {
            // Act
            CNPJ cnpj = ValidNumericCnpj;

            // Assert
            cnpj.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ImplicitOperator_ToString_ShouldWork()
        {
            // Arrange
            CNPJ cnpj = ValidNumericCnpj;

            // Act
            string value = cnpj;

            // Assert
            value.Should().Be(ValidNumericCnpj);
        }

        #endregion
    }
}