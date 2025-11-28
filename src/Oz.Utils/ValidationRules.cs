using FluentValidation;

namespace Oz.Utils;

/// <summary>
/// A collection of reusable FluentValidation rules.
/// </summary>
public static class ValidationRules
{
    /// <summary>
    /// Validator for required string parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string?> IsRequired<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("{PropertyName} is required.");
    }

    /// <summary>
    /// Validator for HTTP methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string?> MustBeAValidHttpMethod<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("{PropertyName} must be a valid HTTP method.");
    }
}
