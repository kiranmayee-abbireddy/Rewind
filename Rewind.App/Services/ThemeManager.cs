using System;
using System.Windows;
using System.Windows.Media;

namespace Rewind.App.Services
{
    public static class ThemeManager
    {
        public static string CurrentTheme { get; private set; } = "Dark";

        public static void ApplyTheme(string theme)
        {
            CurrentTheme = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";

            var dict = Application.Current?.Resources;
            if (dict == null) return;

            if (CurrentTheme == "Light")
            {
                // Luxury Emerald & Champagne Ivory Light Palette
                SetResourceColor(dict, "BgPrimaryColor", "#F6F8F5");      // Pale Sage Ivory
                SetResourceColor(dict, "BgSecondaryColor", "#EDF2EB");    // Soft Jade Parchment
                SetResourceColor(dict, "BgCardColor", "#FFFFFF");         // Pure White Silk Card
                SetResourceColor(dict, "BorderColor", "#D3DFD1");         // Subtle Jade Border
                SetResourceColor(dict, "BorderFocusColor", "#059669");    // Emerald Highlight
                SetResourceColor(dict, "TextPrimaryColor", "#112217");    // Deep Forest Obsidian Text
                SetResourceColor(dict, "TextSecondaryColor", "#4A6B56");  // Sage Subtext
                SetResourceColor(dict, "AccentColor", "#059669");         // Imperial Emerald Accent
                SetResourceColor(dict, "GoldColor", "#D97706");           // Polished Warm Gold
                SetResourceColor(dict, "DangerColor", "#DC2626");
                SetResourceColor(dict, "SuccessColor", "#059669");
                SetResourceColor(dict, "WarningColor", "#D97706");

                // Update Gradient Brushes
                dict["PrimaryAccentGradient"] = CreateGradient("#059669", "#047857");
                dict["LogoOrbGradient"] = CreateGradient("#10B981", "#047857", "#064E3B");
                dict["GoldOrbGradient"] = CreateGradient("#F59E0B", "#D97706", "#92400E");
            }
            else
            {
                // Luxury Emerald & Obsidian Gold Dark Palette
                SetResourceColor(dict, "BgPrimaryColor", "#080E0B");      // Deepest Forest Obsidian
                SetResourceColor(dict, "BgSecondaryColor", "#0E1813");    // Rich Emerald Slate
                SetResourceColor(dict, "BgCardColor", "#14231C");         // Velvet Jade Card
                SetResourceColor(dict, "BorderColor", "#1F352B");         // Polished Jade Line
                SetResourceColor(dict, "BorderFocusColor", "#10B981");    // Radiant Emerald Focus
                SetResourceColor(dict, "TextPrimaryColor", "#F2F9F5");    // Mint Silk White
                SetResourceColor(dict, "TextSecondaryColor", "#7E9F8E");  // Muted Sage Silver
                SetResourceColor(dict, "AccentColor", "#10B981");         // Brilliant Imperial Emerald
                SetResourceColor(dict, "GoldColor", "#F59E0B");           // Luminous Warm Gold
                SetResourceColor(dict, "DangerColor", "#EF4444");
                SetResourceColor(dict, "SuccessColor", "#10B981");
                SetResourceColor(dict, "WarningColor", "#F59E0B");

                // Update Gradient Brushes
                dict["PrimaryAccentGradient"] = CreateGradient("#059669", "#047857");
                dict["LogoOrbGradient"] = CreateGradient("#34D399", "#10B981", "#064E3B");
                dict["GoldOrbGradient"] = CreateGradient("#FBBF24", "#F59E0B", "#B45309");
            }

            // Sync all SolidColorBrushes from updated Colors
            SyncBrush(dict, "BgPrimaryBrush", "BgPrimaryColor");
            SyncBrush(dict, "BgSecondaryBrush", "BgSecondaryColor");
            SyncBrush(dict, "BgCardBrush", "BgCardColor");
            SyncBrush(dict, "BorderBrush", "BorderColor");
            SyncBrush(dict, "TextPrimaryBrush", "TextPrimaryColor");
            SyncBrush(dict, "TextSecondaryBrush", "TextSecondaryColor");
            SyncBrush(dict, "AccentBrush", "AccentColor");
            SyncBrush(dict, "GoldBrush", "GoldColor");
            SyncBrush(dict, "DangerBrush", "DangerColor");
            SyncBrush(dict, "SuccessBrush", "SuccessColor");
            SyncBrush(dict, "WarningBrush", "WarningColor");
        }

        private static void SetResourceColor(ResourceDictionary dict, string key, string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            dict[key] = color;
        }

        private static void SyncBrush(ResourceDictionary dict, string brushKey, string colorKey)
        {
            if (dict[colorKey] is Color col)
            {
                dict[brushKey] = new SolidColorBrush(col);
            }
        }

        private static LinearGradientBrush CreateGradient(string c1, string c2)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c1), 0));
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c2), 1));
            return brush;
        }

        private static LinearGradientBrush CreateGradient(string c1, string c2, string c3)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c1), 0));
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c2), 0.5));
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c3), 1));
            return brush;
        }
    }
}
