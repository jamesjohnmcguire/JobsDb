/////////////////////////////////////////////////////////////////////////////
// <copyright file="MenuHelper.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace FramePFX.Themes.Attached;

using System.Windows;

public static class MenuHelper
{
	public static readonly DependencyProperty UseStretchedContentProperty = DependencyProperty.RegisterAttached("UseStretchedContent", typeof(bool), typeof(MenuHelper), new PropertyMetadata(false));

	public static void SetUseStretchedContent(DependencyObject element, bool value) {
		element.SetValue(UseStretchedContentProperty, value);
	}

	public static bool GetUseStretchedContent(DependencyObject element) {
		return (bool) element.GetValue(UseStretchedContentProperty);
	}
}
