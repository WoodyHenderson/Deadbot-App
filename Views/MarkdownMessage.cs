using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;

namespace DeadBot.Views;

// Native controls, not a browser: no HTML execution, remote images, or automatic navigation.
public sealed class MarkdownMessage : ContentControl
{
    public static readonly StyledProperty<string> MarkdownProperty =
        AvaloniaProperty.Register<MarkdownMessage, string>(nameof(Markdown), "");
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
    private static readonly IBrush TextColor = Brush.Parse("#E2EAF4");
    public string Markdown { get => GetValue(MarkdownProperty); set => SetValue(MarkdownProperty, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MarkdownProperty)
            Content = RenderBlocks(Markdig.Markdown.Parse(Markdown ?? "", Pipeline));
    }

    private static StackPanel RenderBlocks(ContainerBlock blocks)
    {
        var panel = new StackPanel { Spacing = 12 };
        foreach (var block in blocks)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    var title = Text(heading.Inline);
                    title.FontSize = heading.Level switch { 1 => 25, 2 => 21, 3 => 18, _ => 16 };
                    title.FontWeight = FontWeight.SemiBold;
                    title.Margin = new Thickness(0, 8, 0, 2);
                    panel.Children.Add(title);
                    break;
                case ParagraphBlock paragraph:
                    panel.Children.Add(Text(paragraph.Inline));
                    break;
                case CodeBlock code:
                    panel.Children.Add(new Border
                    {
                        Background = Brush.Parse("#10151D"), CornerRadius = new CornerRadius(8), Padding = new Thickness(14),
                        Child = new ScrollViewer
                        {
                            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                            Content = new SelectableTextBlock { Text = code.Lines.ToString(), FontFamily = new FontFamily("Cascadia Mono, Consolas, monospace"), Foreground = Brush.Parse("#9AEBDD"), FontSize = 13 }
                        }
                    });
                    break;
                case ListBlock list:
                    var number = int.TryParse(list.OrderedStart, out var start) ? start : 1;
                    foreach (ListItemBlock item in list)
                    {
                        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("32,*") };
                        row.Children.Add(new TextBlock { Text = list.IsOrdered ? $"{number++}." : "•", Foreground = Brush.Parse("#9AEBDD"), FontSize = 15 });
                        var body = RenderBlocks(item);
                        Grid.SetColumn(body, 1);
                        row.Children.Add(body);
                        panel.Children.Add(row);
                    }
                    break;
                case QuoteBlock quote:
                    panel.Children.Add(new Border { BorderBrush = Brush.Parse("#70DEC9"), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(14, 4), Child = RenderBlocks(quote) });
                    break;
                case ThematicBreakBlock:
                    panel.Children.Add(new Border { Height = 1, Background = Brush.Parse("#49576A"), Margin = new Thickness(0, 6) });
                    break;
                case Table table:
                    var grid = new Grid();
                    var rowIndex = 0;
                    foreach (TableRow row in table)
                    {
                        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                        for (var col = 0; col < row.Count; col++)
                        {
                            if (grid.ColumnDefinitions.Count <= col) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                            var cell = new Border { Padding = new Thickness(12, 8), BorderBrush = Brush.Parse("#49576A"), BorderThickness = new Thickness(0, 0, 0, 1), Background = Brush.Parse(row.IsHeader ? "#263747" : "#19222E"), Child = RenderBlocks((TableCell)row[col]) };
                            Grid.SetColumn(cell, col); Grid.SetRow(cell, rowIndex); grid.Children.Add(cell);
                        }
                        rowIndex++;
                    }
                    panel.Children.Add(new ScrollViewer { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, Content = grid });
                    break;
                case ContainerBlock container:
                    panel.Children.Add(RenderBlocks(container));
                    break;
                case LeafBlock leaf:
                    panel.Children.Add(new SelectableTextBlock { Text = leaf.Lines.ToString(), TextWrapping = TextWrapping.Wrap, Foreground = TextColor });
                    break;
            }
        }
        return panel;
    }

    private static SelectableTextBlock Text(ContainerInline? content)
    {
        var text = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap, Foreground = TextColor, FontSize = 15, LineHeight = 25 };
        if (content is not null) AddInlines(text.Inlines!, content);
        return text;
    }

    private static void AddInlines(InlineCollection target, ContainerInline source)
    {
        foreach (var inline in source)
        {
            switch (inline)
            {
                case LiteralInline literal: target.Add(new Run(literal.Content.ToString())); break;
                case CodeInline code:
                    target.Add(new Run(code.Content) { FontFamily = new FontFamily("Consolas, monospace"), Foreground = Brush.Parse("#9AEBDD"), Background = Brush.Parse("#10151D") }); break;
                case LineBreakInline: target.Add(new LineBreak()); break;
                case EmphasisInline emphasis:
                    var span = new Span();
                    if (emphasis.DelimiterCount >= 2) span.FontWeight = FontWeight.Bold;
                    else span.FontStyle = FontStyle.Italic;
                    AddInlines(span.Inlines, emphasis); target.Add(span); break;
                case LinkInline link:
                    var label = new Span { Foreground = Brush.Parse("#9AEBDD") };
                    AddInlines(label.Inlines, link); target.Add(label);
                    // Expose the destination as selectable text; never open model-generated URLs automatically.
                    if (!string.IsNullOrEmpty(link.Url)) target.Add(new Run($" ({link.Url})"));
                    break;
                case AutolinkInline link: target.Add(new Run(link.Url) { Foreground = Brush.Parse("#9AEBDD") }); break;
                case HtmlInline html: target.Add(new Run(html.Tag)); break;
                case ContainerInline container: AddInlines(target, container); break;
            }
        }
    }
}
