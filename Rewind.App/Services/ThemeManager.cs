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
                // Nordic Minimalist Light Palette (Porcelain, Cool Slate & Neutral Charcoal)
                SetResourceColor(dict, "BgPrimaryColor", "#F8FAFC");      // Crisp Pure Porcelain
                SetResourceColor(dict, "BgSecondaryColor", "#EDF2F7");    // Muted Cool Mist
                SetResourceColor(dict, "BgCardColor", "#FFFFFF");         // Pure Chalk Card
                SetResourceColor(dict, "BorderColor", "#D9E2EC");         // Whisper Line
                SetResourceColor(dict, "BorderFocusColor", "#475569");    // Deep Slate Focus
                SetResourceColor(dict, "TextPrimaryColor", "#0F172A");    // True Charcoal
                SetResourceColor(dict, "TextSecondaryColor", "#64748B");  // Graphite Subtext
                SetResourceColor(dict, "AccentColor", "#0F172A");         // Nordic Dark Accent
                SetResourceColor(dict, "GoldColor", "#D97706");           // Polished Amber
                SetResourceColor(dict, "DangerColor", "#E11D48");
                SetResourceColor(dict, "SuccessColor", "#16A34A");
                SetResourceColor(dict, "WarningColor", "#D97706");

                // Update Gradient Brushes
                dict["PrimaryAccentGradient"] = CreateGradient("#0F172A", "#334155");
                dict["LogoOrbGradient"] = CreateGradient("#475569", "#334155", "#0F172A");
                dict["GoldOrbGradient"] = CreateGradient("#F59E0B", "#D97706", "#92400E");
            }
            else
            {
                // Nordic Minimalist Dark Palette (Matte Slate Graphite & Titanium)
                SetResourceColor(dict, "BgPrimaryColor", "#121418");      // Deep Matte Slate
                SetResourceColor(dict, "BgSecondaryColor", "#181B20");    // Secondary Surface
                SetResourceColor(dict, "BgCardColor", "#1E2229");         // Elevated Titanium Surface
                SetResourceColor(dict, "BorderColor", "#2A303C");         // Hairline Separation
                SetResourceColor(dict, "BorderFocusColor", "#64748B");    // Slate Glow
                SetResourceColor(dict, "TextPrimaryColor", "#F3F5F7");    // Pure Chalk White
                SetResourceColor(dict, "TextSecondaryColor", "#8E98A8");  // Cool Silver Grey
                SetResourceColor(dict, "AccentColor", "#E2E8F0");         // Crisp Platinum White
                SetResourceColor(dict, "GoldColor", "#E5A93C");           // Quiet Warm Gold
                SetResourceColor(dict, "DangerColor", "#E05252");
                SetResourceColor(dict, "SuccessColor", "#38A169");
                SetResourceColor(dict, "WarningColor", "#E5A93C");

                // Update Gradient Brushes
                dict["PrimaryAccentGradient"] = CreateGradient("#F8FAFC", "#E2E8F0");
                dict["LogoOrbGradient"] = CreateGradient("#94A3B8", "#475569", "#1E293B");
                dict["GoldOrbGradient"] = CreateGradient("#FCD34D", "#F59E0B", "#B45309");
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
