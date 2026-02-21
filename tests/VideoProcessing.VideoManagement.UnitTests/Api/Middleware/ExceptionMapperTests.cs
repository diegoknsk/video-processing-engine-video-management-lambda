using FluentAssertions;
using FluentValidation;
using VideoProcessing.VideoManagement.Api.Middleware;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Middleware;

public class ExceptionMapperTests
{
    [Fact]
    public void Map_UnauthorizedAccessException_Returns401Unauthorized()
    {
        var ex = new UnauthorizedAccessException();

        var (statusCode, code, message) = ExceptionMapper.Map(ex);

        statusCode.Should().Be(401);
        code.Should().Be("Unauthorized");
        message.Should().Be("Acesso não autorizado.");
    }

    [Fact]
    public void Map_ArgumentException_Returns400BadRequest()
    {
        var ex = new ArgumentException("Invalid arg");

        var (statusCode, code, message) = ExceptionMapper.Map(ex);

        statusCode.Should().Be(400);
        code.Should().Be("BadRequest");
        message.Should().Be("Requisição inválida.");
    }

    [Fact]
    public void Map_ValidationException_Returns400BadRequest()
    {
        var ex = new ValidationException("Validation failed");

        var (statusCode, code, message) = ExceptionMapper.Map(ex);

        statusCode.Should().Be(400);
        code.Should().Be("BadRequest");
        message.Should().Be("Validation failed");
    }

    [Fact]
    public void Map_KeyNotFoundException_Returns404NotFound()
    {
        var ex = new KeyNotFoundException();

        var (statusCode, code, message) = ExceptionMapper.Map(ex);

        statusCode.Should().Be(404);
        code.Should().Be("NotFound");
        message.Should().Be("Recurso não encontrado.");
    }

    [Fact]
    public void Map_GenericException_Returns500InternalServerError()
    {
        var ex = new Exception("Unexpected");

        var (statusCode, code, message) = ExceptionMapper.Map(ex);

        statusCode.Should().Be(500);
        code.Should().Be("InternalServerError");
        message.Should().Be("Erro interno do servidor.");
    }
}
