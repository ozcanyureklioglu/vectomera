using FluentValidation;
using Helios.Application.Features.Products.Requests;

namespace Helios.Application.Features.Products.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU boş olamaz.")
            .MaximumLength(100).WithMessage("SKU en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.")
            .When(x => x.Description != null);
    }
}
