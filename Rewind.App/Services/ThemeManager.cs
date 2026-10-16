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
            CurrentTheme = theme ?? "Dark";

            var dict = Application.Current?.Resources;
            if (dict == null) return;

            if (string.Equals(CurrentTheme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                // Nordic Minimalist Light Palette (Porcelain, Cool Slate & Neutral Charcoal)
                SetResourceColor(dict, "BgPrimaryColor", "#F8FAFC");
                SetResourceColor(dict, "BgSecondaryColor", "#EDF2F7");
                SetResourceColor(dict, "BgCardColor", "#FFFFFF");
                SetResourceColor(dict, "BorderColor", "#D9E2EC");
                SetResourceColor(dict, "BorderFocusColor", "#475569");
                SetResourceColor(dict, "TextPrimaryColor", "#0F172A");
                SetResourceColor(dict, "TextSecondaryColor", "#64748B");
                SetResourceColor(dict, "AccentColor", "#0F172A");
                SetResourceColor(dict, "GoldColor", "#D97706");
                SetResourceColor(dict, "DangerColor", "#E11D48");
                SetResourceColor(dict, "SuccessColor", "#16A34A");
                SetResourceColor(dict, "WarningColor", "#D97706");

                dict["PrimaryAccentGradient"] = CreateGradient("#0F172A", "#334155");
                dict["LogoOrbGradient"] = CreateGradient("#475569", "#334155", "#0F172A");
                dict["GoldOrbGradient"] = CreateGradient("#F59E0B", "#D97706", "#92400E");
            }
            else if (string.Equals(CurrentTheme, "Emerald", StringComparison.OrdinalIgnoreCase))
            {
                // Cinematic Studio Emerald Obsidian
                SetResourceColor(dict, "BgPrimaryColor", "#080F0C");
                SetResourceColor(dict, "BgSecondaryColor", "#0D1813");
                SetResourceColor(dict, "BgCardColor", "#14241D");
                SetResourceColor(dict, "BorderColor", "#1E352B");
                SetResourceColor(dict, "BorderFocusColor", "#10B981");
                SetResourceColor(dict, "TextPrimaryColor", "#F2F9F5");
                SetResourceColor(dict, "TextSecondaryColor", "#7E9F8E");
                SetResourceColor(dict, "AccentColor", "#10B981");
                SetResourceColor(dict, "GoldColor", "#F59E0B");
                SetResourceColor(dict, "DangerColor", "#EF4444");
                SetResourceColor(dict, "SuccessColor", "#10B981");
                SetResourceColor(dict, "WarningColor", "#F59E0B");

                dict["PrimaryAccentGradient"] = CreateGradient("#059669", "#047857");
                dict["LogoOrbGradient"] = CreateGradient("#34D399", "#10B981", "#064E3B");
                dict["GoldOrbGradient"] = CreateGradient("#FBBF24", "#F59E0B", "#B45309");
            }
            else if (string.Equals(CurrentTheme, "Burgundy", StringComparison.OrdinalIgnoreCase))
            {
                // Cinematic Velvet Burgundy
                SetResourceColor(dict, "BgPrimaryColor", "#140A0D");
                SetResourceColor(dict, "BgSecondaryColor", "#1C0F13");
                SetResourceColor(dict, "BgCardColor", "#27151B");
                SetResourceColor(dict, "BorderColor", "#3E1E28");
                SetResourceColor(dict, "BorderFocusColor", "#F43F5E");
                SetResourceColor(dict, "TextPrimaryColor", "#FFF1F2");
                SetResourceColor(dict, "TextSecondaryColor", "#FDA4AF");
                SetResourceColor(dict, "AccentColor", "#F43F5E");
                SetResourceColor(dict, "GoldColor", "#F59E0B");
                SetResourceColor(dict, "DangerColor", "#E11D48");
                SetResourceColor(dict, "SuccessColor", "#10B981");
                SetResourceColor(dict, "WarningColor", "#F59E0B");

                dict["PrimaryAccentGradient"] = CreateGradient("#E11D48", "#9F1239");
                dict["LogoOrbGradient"] = CreateGradient("#FB7185", "#E11D48", "#881337");
                dict["GoldOrbGradient"] = CreateGradient("#FBBF24", "#F59E0B", "#B45309");
            }
            else
            {
                // Cinematic Dark Studio (Default)
                SetResourceColor(dict, "BgPrimaryColor", "#111317");
                SetResourceColor(dict, "BgSecondaryColor", "#171A1F");
                SetResourceColor(dict, "BgCardColor", "#1E222A");
                SetResourceColor(dict, "BorderColor", "#2B313D");
                SetResourceColor(dict, "BorderFocusColor", "#64748B");
                SetResourceColor(dict, "TextPrimaryColor", "#F8FAFC");
                SetResourceColor(dict, "TextSecondaryColor", "#94A3B8");
                SetResourceColor(dict, "AccentColor", "#F8FAFC");
                SetResourceColor(dict, "GoldColor", "#E5A93C");
                SetResourceColor(dict, "DangerColor", "#E05252");
                SetResourceColor(dict, "SuccessColor", "#38A169");
                SetResourceColor(dict, "WarningColor", "#E5A93C");

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
