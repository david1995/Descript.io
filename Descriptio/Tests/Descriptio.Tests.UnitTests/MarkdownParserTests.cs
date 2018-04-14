﻿using System.Collections.Generic;
using Descriptio.Core.AST;
using Descriptio.Parser;
using Descriptio.Tests.FluentAssertionsExtensions;
using Microsoft.FSharp.Core;
using Xunit;

using static Descriptio.Parser.MarkdownLexer;

// ReSharper disable InconsistentNaming

namespace Descriptio.Tests.UnitTests
{
    [Trait("Project", "Descriptio.Parser")]
    [Trait("Type", "Markdown.TextParser")]
    public class MarkdownParserTests
    {
        public static readonly IEnumerable<object[]> Parser_AtxTitle_ShouldReturnTitle_Data = new[]
        {
            new object[]{ new[] { Token.TitleLevelToken, Token.NewTitleToken("Title 1") }, new TitleAst(text: "Title 1")},
            new object[]{ new[] { Token.TitleLevelToken, Token.TitleLevelToken, Token.TitleLevelToken, Token.NewTitleToken("### Hello World!") }, new TitleAst(text: "### Hello World!", level: 3)},
            new object[]{ new[] { Token.TitleLevelToken, Token.NewTitleToken("Hello World!##"), Token.TitleClosingToken }, new TitleAst(text: "Hello World!##")},
        };

        [Theory(DisplayName = "Parser should parse ATX Title")]
        [MemberData(nameof(Parser_AtxTitle_ShouldReturnTitle_Data))]
        public void Parser_AtxTitle_ShouldReturnTitle(Token[] source, FSharpOption<IAbstractSyntaxTree> expected)
        {
            // Arrange
            var parser = new MarkdownParser.MarkdownParser();

            // Act
            var result = parser.Parse(source);

            // Assert
            result.Should().Be(expected);
        }

        public static readonly IEnumerable<object[]> Parser_TextLine_ShouldReturnTextLine_Data = new[]
        {
            new object[]
            {
                new[] { Token.NewTextToken("This is a text.") },
                new TextParagraphBlock(new[] { new CleanTextInline("This is a text.") })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is an "),
                    Token.EmphasisStartToken,
                    Token.NewTextToken("emphasized"),
                    Token.EmphasisEndToken,
                    Token.NewTextToken(" text.")
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is an "),
                        new EmphasisTextInline("emphasized"),
                        new CleanTextInline(" text.")
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is a "),
                    Token.StrongStartToken,
                    Token.NewTextToken("strong"),
                    Token.StrongEndToken,
                    Token.NewTextToken(" text.")
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is a "),
                        new StrongTextInline("strong"),
                        new CleanTextInline(" text.")
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is a "),
                    Token.InlineCodeStartToken,
                    Token.NewTextToken("code inline"),
                    Token.InlineCodeEndToken,
                    Token.NewTextToken(" text.")
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is a "),
                        new CodeTextInline("code inline"),
                        new CleanTextInline(" text.")
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is a "),
                    Token.InlineCodeStartToken,
                    Token.NewTextToken("code`inline"),
                    Token.InlineCodeEndToken,
                    Token.NewTextToken(" text.")
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is a "),
                        new CodeTextInline("code`inline"),
                        new CleanTextInline(" text.")
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is an "),
                    Token.ImageAltStartToken,
                    Token.NewTextToken("Alternative"),
                    Token.ImageAltEndToken,
                    Token.LinkStartToken,
                    Token.NewTextToken(@"C:\Path\To\Image.jpg"),
                    Token.NewTextToken("Some image"),
                    Token.LinkEndToken,
                    Token.NewTextToken(" image.")
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is an "),
                        new ImageInline(alt: "Alternative", src: @"C:\Path\To\Image.jpg", title: "Some image"),
                        new CleanTextInline(" image.")
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewTextToken("This is a "),
                    Token.LinkTextStartToken,
                    Token.NewTextToken("hyperlink."),
                    Token.LinkTextEndToken,
                    Token.LinkStartToken,
                    Token.NewTextToken(@"http://example.com"),
                    Token.NewTextToken("Hyperlink title"),
                    Token.LinkEndToken,
                },
                new TextParagraphBlock(
                    new IAbstractSyntaxTreeInline[]
                    {
                        new CleanTextInline("This is a "),
                        new HyperlinkInline(text: "hyperlink.", href: @"http://example.com", title: "Hyperlink title"),
                    })
            }
        };

        [Theory(DisplayName = "Parser should parse text line")]
        [MemberData(nameof(Parser_TextLine_ShouldReturnTextLine_Data))]
        public void Parser_TextLine_ShouldReturnTextLine(Token[] source, FSharpOption<IAbstractSyntaxTree> expected)
        {
            // Arrange
            var parser = new MarkdownParser.MarkdownParser();

            // Act
            var result = parser.Parse(source);

            // Assert
            result.Should().Be(expected);
        }

        public static readonly IEnumerable<object[]> Parser_Enumeration_ShouldReturnEnumeration_Data = new[]
        {
            new object[]
            {
                new[]
                {
                    Token.NewEnumerationToken(1),
                    Token.NewTextToken("Hello World!"),
                    Token.NewEnumerationToken(2),
                    Token.NewTextToken("Second point.")
                },
                new EnumerationBlock(
                    items: new []
                    {
                        new EnumerationItem(
                            number: 1,
                            inlines: new[]
                            {
                                new CleanTextInline("Hello World!")
                            }),
                        new EnumerationItem(
                            number: 2,
                            inlines: new[]
                            {
                                new CleanTextInline("Second point.")
                            }),
                    })
            },
            new object[]
            {
                new[]
                {
                    Token.NewUnorderedEnumerationToken(0, '*'),
                    Token.NewTextToken("Hello World!"),
                    Token.NewUnorderedEnumerationToken(0, '*'),
                    Token.NewTextToken("Second point.")
                },
                new UnorderedEnumerationBlock(
                    items: new []
                    {
                        new UnorderedEnumerationItem(
                            indent: 0,
                            bullet: '*',
                            inlines: new[]
                            {
                                new CleanTextInline("Hello World!")
                            }),
                        new UnorderedEnumerationItem(
                            indent: 0,
                            bullet: '*',
                            inlines: new[]
                            {
                                new CleanTextInline("Second point.")
                            }),
                    })
            },
        };

        [Theory(DisplayName = "Parser should parse enumeration")]
        [MemberData(nameof(Parser_Enumeration_ShouldReturnEnumeration_Data))]
        public void Parser_Enumeration_ShouldReturnEnumeration(Token[] source, FSharpOption<IAbstractSyntaxTree> expected)
        {
            // Arrange
            var parser = new MarkdownParser.MarkdownParser();

            // Act
            var result = parser.Parse(source);

            // Assert
            result.Should().Be(expected);
        }

        public static readonly IEnumerable<object[]> Parser_CodeBlock_ShouldReturnCodeBlock_Data = new[]
        {
            new object[]
            {
                new[]
                {
                    Token.CodeBlockStartToken,
                    Token.NewTextToken("I am some code()!"),
                    Token.CodeBlockEndToken
                },
                new CodeBlock(
                    language: string.Empty,
                    lines: new [] { "I am some code()!" },
                    next: null)
            },
            new object[]
            {
                new[]
                {
                    Token.CodeBlockStartToken,
                    Token.NewCodeBlockLanguageToken("c#"),
                    Token.NewTextToken("private void IAmCSharpCode()"),
                    Token.NewTextToken("{"),
                    Token.NewTextToken("}"),
                    Token.CodeBlockEndToken
                },
                new CodeBlock("c#", new[] { "private void IAmCSharpCode()", "{", "}"})
            },
        };

        [Theory(DisplayName = "Parser should parse code blocks")]
        [MemberData(nameof(Parser_CodeBlock_ShouldReturnCodeBlock_Data))]
        public void Parser_CodeBlock_ShouldReturnCodeBlock(Token[] source, FSharpOption<IAbstractSyntaxTree> expected)
        {
            // Arrange
            var parser = new MarkdownParser.MarkdownParser();

            // Act
            var result = parser.Parse(source);

            // Assert
            result.Should().Be(expected);
        }
    }
}
