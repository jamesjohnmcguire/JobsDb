/////////////////////////////////////////////////////////////////////////////
// <copyright file="CornerHelper.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace FramePFX.Themes.Attached;

using System.Windows;

public static class CornerHelper
{
	public static readonly DependencyProperty CornerRadiusProperty =
		DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(CornerHelper), new PropertyMetadata(new CornerRadius(0)));

	public static void SetCornerRadius(DependencyObject element, CornerRadius value) {
		element.SetValue(CornerRadiusProperty, value);
	}

	public static CornerRadius GetCornerRadius(DependencyObject element) {
		return (CornerRadius) element.GetValue(CornerRadiusProperty);
	}
}
