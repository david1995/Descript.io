﻿using System.IO;
using System.Text;
using Descriptio.Core.AST;
using Xunit;

namespace Descriptio.Tests.IntegrationTests.HtmlFormatter
{
    public class HtmlFormatterIntegrationTests
    {
        [Fact]
        public void HtmlFormatter_ShouldYieldDocument()
        {
            var htmlFormatter = new Transform.Html.HtmlFormatter();

            var ast = new TitleAst(
                "Title 1",
                level: 1,
                next: new TextParagraphBlock(new[]
                    {
                        new CleanTextInline("This is a text."),
                    },
                    next: new TitleAst(
                        "Title 2",
                        level: 2,
                        next: new TextParagraphBlock(new IAbstractSyntaxTreeInline[]
                        {
                            new CleanTextInline("This is another text. "),
                            new StrongTextInline("This should be strong."),
                            new CleanTextInline(" "),
                            new EmphasisTextInline("And this should be emphasized."),
                            new CleanTextInline(" "),
                            new CodeTextInline("This should be formatted as code."),
                        },
                            next: new TextParagraphBlock(new IAbstractSyntaxTreeInline[]
                            {
                                new CleanTextInline("Here, we should have a new paragraph "),
                                new HyperlinkInline(text: "with a link", href: "http://example.com", title: "It is a title"),
                                new CleanTextInline("."),
                                new ImageInline(alt: "Alt", src: @"C:\Path\To\An\Image.jpg", title: "It has a title too"),
                            },
                                next: new EnumerationBlock(
                                    items: new[]
                                    {
                                        new EnumerationItem(indent: 0, number: 1, inlines: new[] { new CleanTextInline("This should be item 1.") }),
                                        new EnumerationItem(indent: 0, number: 2, inlines: new[] { new CleanTextInline("This should be the second item.")}),
                                        new EnumerationItem(indent: 0, number: 3, inlines: new[] { new CleanTextInline("Though, this should be item 3.")})
                                    }
                                )
                            )
                        )
                )));

            string expectedResult = @"<h1>Title 1</h1>
<p>
This is a text.</p>
<h2>Title 2</h2>
<p>
This is another text. <strong>This should be strong.</strong> <em>And this should be emphasized.</em> <code>This should be formatted as code.</code></p>
<p>
Here, we should have a new paragraph <a href=""http://example.com"">with a link</a>.<figure>
<img src=""C:\Path\To\An\Image.jpg"" alt=""Alt""/>
<figcaption>It has a title too</figcaption>
</figure>
</p>
<ol>
<li>This should be item 1.</li>
<li>This should be the second item.</li>
<li>Though, this should be item 3.</li>
</ol>
";

            var memoryStream = new MemoryStream();
            htmlFormatter.Transform(ast, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
            {
                string result = streamReader.ReadToEnd();
                Assert.Equal(expectedResult, result);
            }
        }
    }
}
