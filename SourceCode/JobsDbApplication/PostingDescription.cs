/////////////////////////////////////////////////////////////////////////////
// <copyright file="PostingDescription.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Navigation;

/// <summary>
/// Formats plain posting descriptions with clickable web links.
/// </summary>
internal static partial class PostingDescription
{
	/// <summary>
	/// Replaces the description while preserving its text and punctuation.
	/// </summary>
	/// <param name="inlines">The destination paragraph.</param>
	/// <param name="text">The plain description.</param>
	/// <param name="openLink">Opens a validated address.</param>
	public static void Format(
		InlineCollection inlines, string text, Action<Uri> openLink)
	{
		inlines.Clear();
		int position = 0;
		foreach (Match match in WebAddressPattern().Matches(text))
		{
			string addressText = TrimTrailingPunctuation(match.Value);
			Uri? address = PostingLauncher.GetWebAddress(addressText);
			if (address == null)
			{
				continue;
			}

			AppendText(inlines, text[position..match.Index]);
			inlines.Add(CreateLink(addressText, address, openLink));
			position = match.Index + addressText.Length;
		}

		AppendText(inlines, text[position..]);
	}

	[GeneratedRegex(
		"""\bhttps?://[^\s<>"“”]+""",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex WebAddressPattern();

	private static void AppendText(InlineCollection inlines, string text)
	{
		if (text.Length > 0)
		{
			inlines.Add(new Run(text));
		}
	}

	private static Hyperlink CreateLink(
		string text, Uri address, Action<Uri> openLink)
	{
		Hyperlink link = new Hyperlink(new Run(text));
		link.NavigateUri = address;
		link.SetResourceReference(
			TextElement.ForegroundProperty, "AccentTextFillColorPrimaryBrush");
		link.RequestNavigate += (sender, arguments) =>
		{
			arguments.Handled = true;
			openLink(arguments.Uri);
		};
		return link;
	}

	private static string TrimTrailingPunctuation(string text)
	{
		string address = text.TrimEnd('.', ',', ';', ':', '!', '?', '\'');
		while (address.Length > 0 && HasUnmatchedClosingMark(address))
		{
			address = address[..^1];
		}

		return address;
	}

	private static bool HasUnmatchedClosingMark(string text)
	{
		char closing = text[^1];
		char opening = closing switch
		{
			')' => '(',
			']' => '[',
			'}' => '{',
			_ => '\0'
		};

		return opening != '\0' &&
			text.Count(character => character == closing) >
			text.Count(character => character == opening);
	}
}
