using Application.DTOs;
using Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests;

public sealed class ValidatorsTests
{
    private readonly CreateProductRequestValidator _createProductValidator = new();
    private readonly UpdateProductRequestValidator _updateProductValidator = new();
    private readonly CreateItemRequestValidator _createItemValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    [Fact]
    public void CreateProduct_Fails_WhenNameEmpty()
    {
        var result = _createProductValidator.TestValidate(new CreateProductRequest("", null, null));
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void CreateProduct_Fails_WhenNameTooLong()
    {
        var longName = new string('a', 256);
        var result = _createProductValidator.TestValidate(new CreateProductRequest(longName, null, null));
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void CreateProduct_Passes_WithValidName()
    {
        var result = _createProductValidator.TestValidate(new CreateProductRequest("Laptop", null, null));
        result.ShouldNotHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void UpdateProduct_Fails_WhenNameEmpty()
    {
        var result = _updateProductValidator.TestValidate(new UpdateProductRequest("", null));
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void CreateItem_Fails_WhenQuantityNegative()
    {
        var result = _createItemValidator.TestValidate(new CreateItemRequest(-1));
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void CreateItem_Passes_WhenQuantityZeroOrPositive()
    {
        var result = _createItemValidator.TestValidate(new CreateItemRequest(0));
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Login_Fails_WhenPasswordTooShort()
    {
        var result = _loginValidator.TestValidate(new LoginRequest("user", "abc"));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_Fails_WhenUserNameEmpty()
    {
        var result = _loginValidator.TestValidate(new LoginRequest("", "password123"));
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }
}
