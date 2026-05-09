using Moq;

namespace GenomeMCP.Tests;

public class McpServiceTests
{
    // --- Tool Tests ---

    [Fact]
    public async Task ExecuteGnomadQuery_ValidQuery_ReturnsRawJson()
    {
        // Arrange
        var expectedJson = "{\"data\": {\"gene\": {\"symbol\": \"BRCA1\"}}}";
        var mockClient = new Mock<IGnomadClient>();
        mockClient
            .Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJson);

        var service = new GnomADMcpService(mockClient.Object);
        var tool = new GnomADTools(service);

        // Act: Use the new ExecuteGnomadQuery method
        var query = "{ gene(gene_symbol: \"BRCA1\", reference_genome: GRCh38) { symbol } }";
        var result = await tool.ExecuteGnomadQuery(query);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"symbol\": \"BRCA1\"", result);
    }

    [Fact]
    public async Task IntrospectGnomadSchema_WithTypeName_ExecutesTypeQuery()
    {
        // Arrange: Test the introspection tool directly
        var expectedJson = "{\"data\": {\"__type\": {\"name\": \"Gene\"}}}";
        var mockClient = new Mock<IGnomadClient>();

        // Verify it formats the specific type query correctly
        mockClient
            .Setup(c => c.ExecuteQueryAsync(It.Is<string>(q => q.Contains("__type(name: \"Gene\")")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJson);

        var service = new GnomADMcpService(mockClient.Object);
        var tool = new GnomADTools(service);

        // Act
        var result = await tool.IntrospectGnomadSchema("Gene");

        // Assert
        Assert.Equal(expectedJson, result);
    }

    // --- Service Tests ---

    [Fact]
    public async Task ExecuteRawQueryAsync_ValidQuery_PassesThroughJsonData()
    {
        // Arrange
        var expectedJson = "{\"data\": {\"variant_search\": [{\"variant_id\": \"17-43124030-C-G\"}]}}";
        var mockClient = new Mock<IGnomadClient>();
        mockClient
            .Setup(c => c.ExecuteQueryAsync("TEST_QUERY", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJson);

        var service = new GnomADMcpService(mockClient.Object);

        // Act
        var result = await service.ExecuteRawQueryAsync("TEST_QUERY");

        // Assert
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public async Task ExecuteRawQueryAsync_GraphQLError_ReturnsErrorJson()
    {
        // Arrange
        var errorJson = "{\"errors\":[{\"message\":\"Cannot query field \\\"bad_field\\\"\"}]}";
        var mockClient = new Mock<IGnomadClient>();
        mockClient
            .Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorJson);

        var service = new GnomADMcpService(mockClient.Object);

        // Act
        var result = await service.ExecuteRawQueryAsync("{ bad_query }");

        // Assert
        Assert.Contains("errors", result);
        Assert.Contains("bad_field", result);
    }

    [Fact]
    public async Task ExecuteRawQueryAsync_PayloadTooLarge_ReturnsTruncationError()
    {
        // Arrange: Create a fake JSON string that exceeds the 50,000 limit
        var massiveJson = new string('x', 50001);
        var mockClient = new Mock<IGnomadClient>();
        mockClient
            .Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(massiveJson);

        var service = new GnomADMcpService(mockClient.Object);

        // Act
        var result = await service.ExecuteRawQueryAsync("{ huge_query }");

        // Assert: Verify the safety valve caught it and returned the custom GraphQL error
        Assert.Contains("PAYLOAD TOO LARGE", result);
        Assert.Contains("50001", result);
    }

    // --- Client Integration Tests ---

    [Fact]
    public async Task ExecuteQueryAsync_GnomadClientRealQuery_ReturnsLiveApiData()
    {
        // Arrange: Setup the real HTTP Client
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://gnomad.broadinstitute.org/api")
        };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "GenomeMCP-TestClient/1.0");

        var realClient = new GnomadClient(httpClient);

        var query = @"
            query VariantsInGene {
              gene(gene_symbol: ""BRCA1"", reference_genome: GRCh38) {
                symbol
                gene_id
              }
            }";

        // Act
        var result = await realClient.ExecuteQueryAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"BRCA1\"", result);
        Assert.Contains("\"ENSG00000012048\"", result);
    }
}