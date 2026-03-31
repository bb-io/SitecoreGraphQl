using Apps.SitecoreGraphQl.Actions;
using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Utils;
using RestSharp;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl;

[TestClass]
public class SearchContentReadOnlyTests : TestBase
{
    private const string RootPath = "/sitecore/content/GoTo/LogMeIn/Home/Features/resolve/New_Data";
    private const string Language = "en";
    private const string FilterFieldName = "status";
    private const string FilterFieldValue = "in translation";

    [TestMethod]
    public async Task SearchContent_DefaultFieldFiltering_MatchesCurrentGraphQlBehavior()
    {
        var rootItemId = await GetItemIdByPath(RootPath);
        var client = new Client(CredentialsProviders.ToList());
        var rawItems = await client.SearchContentAsync(
            new SearchContentParams(
                Language,
                [
                    new CriteriaDto
                    {
                        Field = "_path",
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = rootItemId
                    }
                ],
                [
                    new CriteriaDto
                    {
                        Field = FilterFieldName,
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = FilterFieldValue
                    }
                ]),
            CredentialsProviders);

        var action = new ContentActions(InvocationContext, FileManager);
        var actionResult = await action.SearchContent(new SearchContentRequest
        {
            Language = Language,
            RootPath = RootPath,
            FieldNames = [FilterFieldName],
            FieldValues = [FilterFieldValue]
        }, new DateFilters());

        Assert.IsTrue(rawItems.Count > 0, "Expected raw GraphQL search to return items for the reproduction scenario.");
        Assert.AreEqual(rawItems.Count, actionResult.Items.Count(), "Search content should not drop GraphQL matches during client-side filtering.");
        CollectionAssert.AreEquivalent(
            rawItems.Select(item => item.Id).ToList(),
            actionResult.Items.Select(item => item.Id).ToList(),
            "Search content should return the same items as the raw GraphQL search for this scenario.");
    }

    [TestMethod]
    public async Task SearchContent_RawGraphQl_SearchAndWildcardReturnSameFuzzyResults()
    {
        var rootItemId = await GetItemIdByPath(RootPath);
        var client = new Client(CredentialsProviders.ToList());

        var wildcardItems = await client.SearchContentAsync(
            new SearchContentParams(
                Language,
                [
                    new CriteriaDto
                    {
                        Field = "_path",
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = rootItemId
                    }
                ],
                [
                    new CriteriaDto
                    {
                        Field = FilterFieldName,
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = FilterFieldValue
                    }
                ]),
            CredentialsProviders);

        var searchItems = await client.SearchContentAsync(
            new SearchContentParams(
                Language,
                [
                    new CriteriaDto
                    {
                        Field = "_path",
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = rootItemId
                    }
                ],
                [
                    new CriteriaDto
                    {
                        Field = FilterFieldName,
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = FilterFieldValue
                    }
                ]),
            CredentialsProviders);

        Assert.AreEqual(wildcardItems.Count, searchItems.Count);
        CollectionAssert.AreEquivalent(
            wildcardItems.Select(item => item.Id).ToList(),
            searchItems.Select(item => item.Id).ToList(),
            "Sitecore GraphQL search behavior should be the same for SEARCH and WILDCARD in the reproduction scenario.");
    }

    [TestMethod]
    public async Task SearchContent_UseExactFieldValueFiltering_ReturnsScopeItemsWithExactMatches()
    {
        var rootItemId = await GetItemIdByPath(RootPath);
        var client = new Client(CredentialsProviders.ToList());
        var scopeItems = await client.SearchContentAsync(
            new SearchContentParams(
                Language,
                [
                    new CriteriaDto
                    {
                        Field = "_path",
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = rootItemId
                    }
                ]),
            CredentialsProviders);
        var expectedItems = scopeItems
            .Where(item => SearchFieldValueMatcher.MatchesExactFieldValue(item.Fields.Nodes, FilterFieldName, FilterFieldValue))
            .ToList();

        var action = new ContentActions(InvocationContext, FileManager);
        var actionResult = await action.SearchContent(new SearchContentRequest
        {
            Language = Language,
            RootPath = RootPath,
            FieldNames = [FilterFieldName],
            FieldValues = [FilterFieldValue],
            UseExactFieldValueFiltering = true
        }, new DateFilters());

        CollectionAssert.AreEquivalent(
            expectedItems.Select(item => item.Id).ToList(),
            actionResult.Items.Select(item => item.Id).ToList(),
            "Exact field filtering should be applied locally to the scope results.");
    }

    [TestMethod]
    public async Task SearchContent_UseExactFieldValueFiltering_RemovesSitecoreSearchFalsePositives()
    {
        var rootItemId = await GetItemIdByPath(RootPath);
        var client = new Client(CredentialsProviders.ToList());
        var rawItems = await client.SearchContentAsync(
            new SearchContentParams(
                Language,
                [
                    new CriteriaDto
                    {
                        Field = "_path",
                        CriteriaType = "SEARCH",
                        Operator = "MUST",
                        Value = rootItemId
                    }
                ],
                [
                    new CriteriaDto
                    {
                        Field = FilterFieldName,
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = FilterFieldValue
                    }
                ]),
            CredentialsProviders);
        var rawFalsePositives = rawItems
            .Where(item => !SearchFieldValueMatcher.MatchesExactFieldValue(item.Fields.Nodes, FilterFieldName, FilterFieldValue))
            .Select(item => item.Id)
            .ToList();

        var action = new ContentActions(InvocationContext, FileManager);
        var actionResult = await action.SearchContent(new SearchContentRequest
        {
            Language = Language,
            RootPath = RootPath,
            FieldNames = [FilterFieldName],
            FieldValues = [FilterFieldValue],
            UseExactFieldValueFiltering = true
        }, new DateFilters());

        Assert.IsTrue(rawFalsePositives.Count > 0, "Expected the Sitecore search reproduction data set to contain false positives.");
        Assert.IsTrue(actionResult.Items.All(item => !rawFalsePositives.Contains(item.Id)));
    }

    [TestMethod]
    public void SearchFieldValueMatcher_MatchesStructuredAndFormattedValues()
    {
        Assert.IsTrue(SearchFieldValueMatcher.Matches(" In   Translation ", FilterFieldValue));
        Assert.IsTrue(SearchFieldValueMatcher.Matches("[\"in translation\"]", FilterFieldValue));
        Assert.IsTrue(SearchFieldValueMatcher.Matches("ready for review|in translation", FilterFieldValue));
        Assert.IsTrue(SearchFieldValueMatcher.Matches("in translation -03/20/2026 11:42", FilterFieldValue));
    }

    [TestMethod]
    public void SearchFieldValueMatcher_IsCaseInsensitive_AndAvoidsFalsePositives()
    {
        Assert.IsTrue(SearchFieldValueMatcher.Matches("IN TRANSLATION", FilterFieldValue));
        Assert.IsFalse(SearchFieldValueMatcher.Matches("translated -03/27/2026 15:11", FilterFieldValue));
        Assert.IsFalse(SearchFieldValueMatcher.Matches("not in translation", FilterFieldValue));
        Assert.IsFalse(SearchFieldValueMatcher.Matches("translation in progress", FilterFieldValue));
    }

    [TestMethod]
    public void SearchFieldValueMatcher_MatchesExactFieldValue_ByFieldNameOrTemplateFieldKey()
    {
        var byNameFields = new[]
        {
            new Apps.SitecoreGraphQl.Models.Responses.FieldResponse
            {
                Name = "Status",
                Value = " In   Translation "
            }
        };
        var byTemplateKeyFields = new[]
        {
            new Apps.SitecoreGraphQl.Models.Responses.FieldResponse
            {
                Name = "Workflow state",
                Value = "[\"in translation\"]",
                TemplateField = new Apps.SitecoreGraphQl.Models.Responses.TemplateFieldResponse
                {
                    Key = "status"
                }
            }
        };

        Assert.IsTrue(SearchFieldValueMatcher.MatchesExactFieldValue(byNameFields, FilterFieldName, FilterFieldValue));
        Assert.IsTrue(SearchFieldValueMatcher.MatchesExactFieldValue(byTemplateKeyFields, FilterFieldName, FilterFieldValue));
    }

    [TestMethod]
    public void SearchFieldValueMatcher_MissingFieldDoesNotOverrideGraphQlMatch()
    {
        var fields = new[]
        {
            new Apps.SitecoreGraphQl.Models.Responses.FieldResponse
            {
                Name = "title",
                Value = "Example"
            }
        };

        Assert.IsTrue(SearchFieldValueMatcher.Matches(fields, FilterFieldName, FilterFieldValue));
    }

    [TestMethod]
    public void SearchFieldValueMatcher_MatchesExactFieldValue_RequiresMatchingField()
    {
        var fields = new[]
        {
            new Apps.SitecoreGraphQl.Models.Responses.FieldResponse
            {
                Name = "title",
                Value = "Example"
            }
        };

        Assert.IsFalse(SearchFieldValueMatcher.MatchesExactFieldValue(fields, FilterFieldName, FilterFieldValue));
    }

    [TestMethod]
    public void SearchItemsWithCriteriasQuery_OmitsEmptySort_AndEscapesValues()
    {
        var query = GraphQlQueries.SearchItemsWithCriteriasQuery(
            [
                new CriteriaDto
                {
                    Field = "_path",
                    CriteriaType = "SEARCH",
                    Operator = "MUST",
                    Value = "item\\root\"quoted"
                }
            ],
            [
                new CriteriaDto
                {
                    Field = "status",
                    CriteriaType = "WILDCARD",
                    Operator = "MUST",
                    Value = FilterFieldValue
                }
            ]);

        Assert.IsFalse(query.Contains("sort: [  ]", StringComparison.Ordinal));
        Assert.IsFalse(query.Contains("sort: [ ]", StringComparison.Ordinal));
        StringAssert.Contains(query, "subStatements: [ { operator: MUST, criteria: [ { field: \"status\", criteriaType: WILDCARD, operator: MUST, value: \"in translation\" } ] } ]");
        StringAssert.Contains(query, "value: \"item\\\\root\\\"quoted\"");
    }

    private async Task<string> GetItemIdByPath(string path)
    {
        var request = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlQueries.GetItemByPathQuery(path)
            });

        var result = await new Client(CredentialsProviders.ToList()).ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(request);
        return result.Content?.Id ?? throw new AssertFailedException($"Item path '{path}' was not found.");
    }
}
