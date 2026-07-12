using FluentValidation;
using Helios.Application.Features.Products.Requests;

namespace Helios.Application.Features.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU boş olamaz.")
            .MaximumLength(100).WithMessage("SKU en fazla 100 karakter olabilir.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Marka seçimi zorunludur.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.")
            .When(x => x.Description != null);
    }
}
