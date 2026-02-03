/////////////////////////////////////////////////////////////////////////////
// <copyright file="TextHinting.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace FramePFX.Themes.Attached;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

/// <summary>
/// An attached property class for showing a hint when a text box is empty. Set the text box's Tag property to the text
/// </summary>
public static class TextHinting
{
	public static readonly DependencyProperty ShowWhenFocusedProperty =
		DependencyProperty.RegisterAttached(
			"ShowWhenFocused",
			typeof(bool),
			typeof(TextHinting),
			new FrameworkPropertyMetadata(false));

	public static void SetShowWhenFocused(Control control, bool value)
	{
		if (control is TextBoxBase || control is PasswordBox)
		{
			control.SetValue(ShowWhenFocusedProperty, value);
		}

		throw new ArgumentException("Control was not a textbox", nameof(control));
	}

	public static bool GetShowWhenFocused(Control control)
	{
		if (control is TextBoxBase || control is PasswordBox)
		{
			return (bool) control.GetValue(ShowWhenFocusedProperty);
		}

		throw new ArgumentException("Control was not a textbox", nameof(control));
	}
}
