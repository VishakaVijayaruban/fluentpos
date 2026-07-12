using System;
using FakeItEasy;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Modules.Sales.Core.Features.Sales.Commands;
using FluentPOS.Modules.Sales.Core.Features.Sales.Commands.Validators;
using Microsoft.Extensions.Localization;
using Xunit;

namespace FluentPOS.Modules.Sales.Core.Tests.Features.Sales.Commands.Validators.Tests
{
    public class RegisterSaleCommandValidatorShould
    {
        private static RegisterSaleCommandValidator CreateValidator()
        {
            var localizer = A.Fake<IStringLocalizer<RegisterSaleCommandValidator>>();
            A.CallTo(() => localizer[A<string>._])
                .ReturnsLazily((string name) => new LocalizedString(name, name));
            return new RegisterSaleCommandValidator(localizer);
        }

        [Fact]
        public void Passes_when_command_is_valid()
        {
            // Arrange
            var validator = CreateValidator();
            var command = new RegisterSaleCommand
            {
                CartId = Guid.NewGuid(),
                PaymentType = PaymentType.Cash,
                TenderedAmount = 25m
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Fails_when_cart_id_is_empty()
        {
            // Arrange
            var validator = CreateValidator();
            var command = new RegisterSaleCommand { CartId = Guid.Empty };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterSaleCommand.CartId));
        }

        [Fact]
        public void Fails_when_tendered_amount_is_negative()
        {
            // Arrange
            var validator = CreateValidator();
            var command = new RegisterSaleCommand
            {
                CartId = Guid.NewGuid(),
                TenderedAmount = -0.01m
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterSaleCommand.TenderedAmount));
        }

        [Fact]
        public void Fails_when_payment_type_is_not_defined()
        {
            // Arrange
            var validator = CreateValidator();
            var command = new RegisterSaleCommand
            {
                CartId = Guid.NewGuid(),
                PaymentType = (PaymentType)99
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterSaleCommand.PaymentType));
        }
    }
}
