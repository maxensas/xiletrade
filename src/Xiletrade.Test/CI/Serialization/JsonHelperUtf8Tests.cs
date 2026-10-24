using FluentAssertions;
using System.Text;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Test.CI.Serialization;

public class JsonHelperUtf8Tests
{
    private readonly JsonHelper _json = new();

    private const string Plain = "{\"error\":{\"code\":3,\"message\":\"caf\u00e9 \u20ac\"}}";
    private const string BadStashName = "{\"error\":{\"message\":\"bad\\\\\",\"code\":3}}"; // message ends with two backslashes

    [Fact]
    public void _01_Utf8_Bytes_Should_Be_Deserialized()
    {
        var response = _json.Deserialize<NetResponse>(Encoding.UTF8.GetBytes(Plain));

        response.Error.Message.Should().Be("caf\u00e9 \u20ac");
        response.Error.Code.Should().Be(3);
    }

    [Fact]
    public void _02_Byte_Order_Mark_Should_Be_Skipped()
    {
        byte[] bytes = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(Plain)];

        _json.Deserialize<NetResponse>(bytes).Error.Code.Should().Be(3);
    }

    [Fact]
    public void _03_Known_Bad_Data_Should_Be_Fixed_Only_On_Demand()
    {
        var bytes = Encoding.UTF8.GetBytes(BadStashName);

        _json.Deserialize<NetResponse>(bytes, replace: true).Error.Message.Should().Be("bad");
        _json.Deserialize<NetResponse>(bytes).Error.Message.Should().Be("bad\\");
    }

    [Fact]
    public void _04_Fix_Should_Not_Change_A_Correct_Json()
    {
        var response = _json.Deserialize<NetResponse>(Encoding.UTF8.GetBytes(Plain), replace: true);

        response.Error.Message.Should().Be("caf\u00e9 \u20ac");
        response.Error.Code.Should().Be(3);
    }

    [Fact]
    public void _05_Serialized_Bytes_Should_Be_Deserialized_Back()
    {
        var response = new NetResponse { Error = new NetError { Code = 3, Message = "caf\u00e9 \u20ac" } };

        var bytes = _json.SerializeToUtf8<NetResponse>(response);

        _json.Deserialize<NetResponse>(bytes).Error.Message.Should().Be("caf\u00e9 \u20ac");
    }

    [Fact]
    public void _06_Serialization_Should_Keep_The_Non_Breaking_Space()
    {
        var response = new NetResponse { Error = new NetError { Message = "a\u00A0b" } };

        var text = Encoding.UTF8.GetString(_json.SerializeToUtf8<NetResponse>(response));

        text.Should().Contain("a\u00A0b").And.NotContain("\\u00A0");
    }

    [Fact]
    public void _07_Serialization_Without_Replacement_Should_Give_Valid_Json()
    {
        var response = new NetResponse { Error = new NetError { Code = 3, Message = "abc" } };

        var bytes = _json.SerializeToUtf8<NetResponse>(response, replace: false);

        _json.Deserialize<NetResponse>(bytes).Error.Code.Should().Be(3);
    }
}
