using OrukModels.Models;
using OrukTransformer.Core;

namespace OrukTransformer.Core.Tests;

public class PlainTextNormalizationTests
{
    [Fact]
    public void ToPlainText_UnicodeEscapedHtml_ReturnsPlainText()
    {
        var input = @"\u003Cp\u003EDuring bank holidays\u003C/p\u003E";

        var result = OrukPlainText.ToPlainText(input);

        Assert.Equal("During bank holidays", result);
    }

    [Fact]
    public void ToPlainText_DoubleEscapedUnicodeAndEntityHtml_ReturnsPlainText()
    {
        var input = @"\\u003Cp\\u003EHello &amp; welcome\\u003C/p\\u003E";

        var result = OrukPlainText.ToPlainText(input);

        Assert.Equal("Hello & welcome", result);
    }

    [Fact]
    public void ToPlainText_HtmlCommentsAndNbsp_RemovesNoise()
    {
        var input = "<p><!--StartFragment -->A&nbsp;B</p>";

        var result = OrukPlainText.ToPlainText(input);

        Assert.Equal("A B", result);
    }

    [Fact]
    public void DescriptionPlain_ServiceProperty_ReturnsNormalizedValue()
    {
        var service = new OrukService { Id = "s1", Name = "Service", Description = "<p>Text</p>" };

        var result = service.DescriptionPlain();

        Assert.Equal("Text", result);
    }
}
