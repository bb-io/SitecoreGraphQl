using System.Text.Json;
using System.Text.RegularExpressions;
using Apps.SitecoreGraphQl.Models.Responses;

namespace Apps.SitecoreGraphQl.Utils;

public static partial class SearchFieldValueMatcher
{
    public static bool Matches(string? actualValue, string? expectedValue)
    {
        if (string.IsNullOrWhiteSpace(expectedValue))
        {
            return string.IsNullOrWhiteSpace(actualValue);
        }

        if (string.IsNullOrWhiteSpace(actualValue))
        {
            return false;
        }

        var normalizedExpectedValue = Normalize(expectedValue);
        return GetCandidateValues(actualValue)
            .Select(Normalize)
            .Any(candidate => candidate.Equals(normalizedExpectedValue, StringComparison.OrdinalIgnoreCase));
    }

    public static bool Matches(IEnumerable<FieldResponse> fields, string fieldName, string expectedValue)
    {
        var candidateValues = GetCandidateFieldValues(fields, fieldName).ToList();
        if (candidateValues.Count == 0)
        {
            return true;
        }

        return candidateValues.Any(value => Matches(value, expectedValue));
    }

    public static bool MatchesExactFieldValue(IEnumerable<FieldResponse> fields, string fieldName, string expectedValue)
    {
        var candidateValues = GetCandidateFieldValues(fields, fieldName).ToList();
        if (candidateValues.Count == 0)
        {
            return false;
        }

        return candidateValues.Any(value => Matches(value, expectedValue));
    }

    public static IReadOnlyCollection<string> GetCandidateFieldValues(IEnumerable<FieldResponse> fields, string fieldName)
    {
        return fields
            .Where(field => FieldMatches(field, fieldName))
            .Select(field => field.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    private static IEnumerable<string> GetCandidateValues(string actualValue)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in GetRawCandidateValues(actualValue))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var trimmedCandidate = candidate.Trim();
            if (seen.Add(trimmedCandidate))
            {
                yield return trimmedCandidate;
            }
        }
    }

    private static IEnumerable<string> GetRawCandidateValues(string actualValue)
    {
        yield return actualValue;

        foreach (var splitValue in SplitValues(actualValue))
        {
            yield return splitValue;
        }

        foreach (var jsonValue in ExtractJsonStringValues(actualValue))
        {
            yield return jsonValue;
        }
    }

    private static IEnumerable<string> SplitValues(string value)
    {
        return value.Split(['|', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static IEnumerable<string> ExtractJsonStringValues(string value)
    {
        List<string> values = [];

        try
        {
            using var document = JsonDocument.Parse(value);
            values.AddRange(ExtractJsonStringValues(document.RootElement));
        }
        catch (JsonException)
        {
        }

        return values;
    }

    private static IEnumerable<string> ExtractJsonStringValues(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                yield return element.GetString() ?? string.Empty;
                yield break;
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                {
                    foreach (var candidate in ExtractJsonStringValues(child))
                    {
                        yield return candidate;
                    }
                }

                yield break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var candidate in ExtractJsonStringValues(property.Value))
                    {
                        yield return candidate;
                    }
                }

                yield break;
        }
    }

    private static string Normalize(string value)
    {
        var normalizedValue = value.Trim().Trim('"');
        normalizedValue = MultipleWhitespaceRegex().Replace(normalizedValue, " ");
        normalizedValue = TrailingTimestampRegex().Replace(normalizedValue, string.Empty).Trim();
        return MultipleWhitespaceRegex().Replace(normalizedValue, " ");
    }

    private static bool FieldMatches(FieldResponse field, string fieldName)
    {
        return field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
               || field.TemplateField.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
               || field.TemplateField.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
               || field.TemplateField.Title.Equals(fieldName, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    [GeneratedRegex(@"\s*-\d{1,2}/\d{1,2}/\d{4}\s+\d{1,2}:\d{2}(?::\d{2})?\s*$")]
    private static partial Regex TrailingTimestampRegex();
}
