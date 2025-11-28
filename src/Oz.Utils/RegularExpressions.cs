using System.Text.RegularExpressions;

namespace Oz.Utils;

internal static partial class RegularExpressions
{
    [GeneratedRegex(@"^[A-Z]+$")]
    public static partial Regex UpperCaseLetters();
}