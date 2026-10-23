using System;
using System.Windows;
using System.Windows.Media;

namespace Rewind.App.Services
{
    public static class ThemeManager
    {
        public static string CurrentPalette { get; private set; } = "Slate";
        public static bool CurrentIsDark { get; private set; } = true;
        public static string CurrentTheme => $"{CurrentPalette}_{(CurrentIsDark ? "Dark" : "Light")}";

        public static void ApplyTheme(string palette, bool isDark)
        {
            CurrentPalette = string.IsNullOrWhiteSpace(palette) ? "Slate" : palette;
            CurrentIsDark = isDark;

            var dict = Application.Current?.Resources;
            if (dict == null) return;

            string bgPrimary, bgSecondary, bgCard, border, borderFocus;
            string textPrimary, textSecondary, accent, gold, danger, success, warning;
            string grad1, grad2, logo1, logo2, logo3;

            switch (CurrentPalette.ToLowerInvariant())
            {
                case "emerald":
                    if (isDark)
                    {
                        // Studio Emerald Obsidian (Dark)
                        bgPrimary = "#080F0C";
                        bgSecondary = "#0D1813";
                        bgCard = "#14241D";
                        border = "#1E352B";
                        borderFocus = "#10B981";
                        textPrimary = "#F2F9F5";
                        textSecondary = "#7E9F8E";
                        accent = "#10B981";
                        gold = "#F59E0B";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#10B981";
                        grad2 = "#059669";
                        logo1 = "#34D399";
                        logo2 = "#10B981";
                        logo3 = "#064E3B";
                    }
                    else
                    {
                        // Fresh Mint Porcelain (Light)
                        bgPrimary = "#F0FDF4";
                        bgSecondary = "#DCFCE7";
                        bgCard = "#FFFFFF";
                        border = "#BBF7D0";
                        borderFocus = "#059669";
                        textPrimary = "#064E3B";
                        textSecondary = "#047857";
                        accent = "#059669";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#059669";
                        grad2 = "#047857";
                        logo1 = "#10B981";
                        logo2 = "#059669";
                        logo3 = "#064E3B";
                    }
                    break;

                case "burgundy":
                    if (isDark)
                    {
                        // Velvet Burgundy Noir (Dark)
                        bgPrimary = "#140A0D";
                        bgSecondary = "#1C0F13";
                        bgCard = "#27151B";
                        border = "#3E1E28";
                        borderFocus = "#F43F5E";
                        textPrimary = "#FFF1F2";
                        textSecondary = "#FDA4AF";
                        accent = "#F43F5E";
                        gold = "#F59E0B";
                        danger = "#E11D48";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#F43F5E";
                        grad2 = "#BE123C";
                        logo1 = "#FB7185";
                        logo2 = "#E11D48";
                        logo3 = "#881337";
                    }
                    else
                    {
                        // Rose Quartz Porcelain (Light)
                        bgPrimary = "#FFF1F2";
                        bgSecondary = "#FFE4E6";
                        bgCard = "#FFFFFF";
                        border = "#FECDD3";
                        borderFocus = "#E11D48";
                        textPrimary = "#881337";
                        textSecondary = "#9F1239";
                        accent = "#E11D48";
                        gold = "#D97706";
                        danger = "#BE123C";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#E11D48";
                        grad2 = "#9F1239";
                        logo1 = "#FB7185";
                        logo2 = "#E11D48";
                        logo3 = "#881337";
                    }
                    break;

                case "amber":
                    if (isDark)
                    {
                        // Golden Obsidian Studio (Dark)
                        bgPrimary = "#120F08";
                        bgSecondary = "#1A150D";
                        bgCard = "#241D12";
                        border = "#3D301E";
                        borderFocus = "#F59E0B";
                        textPrimary = "#FEF3C7";
                        textSecondary = "#D97706";
                        accent = "#F59E0B";
                        gold = "#FBBF24";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#F59E0B";
                        grad2 = "#D97706";
                        logo1 = "#FDE68A";
                        logo2 = "#F59E0B";
                        logo3 = "#78350F";
                    }
                    else
                    {
                        // Warm Honey Sunlight (Light)
                        bgPrimary = "#FFFBEB";
                        bgSecondary = "#FEF3C7";
                        bgCard = "#FFFFFF";
                        border = "#FDE68A";
                        borderFocus = "#D97706";
                        textPrimary = "#78350F";
                        textSecondary = "#92400E";
                        accent = "#D97706";
                        gold = "#B45309";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#D97706";
                        grad2 = "#B45309";
                        logo1 = "#F59E0B";
                        logo2 = "#D97706";
                        logo3 = "#78350F";
                    }
                    break;

                case "sapphire":
                    if (isDark)
                    {
                        // Midnight Sapphire Studio (Dark)
                        bgPrimary = "#090D16";
                        bgSecondary = "#0F1624";
                        bgCard = "#162035";
                        border = "#20304E";
                        borderFocus = "#38BDF8";
                        textPrimary = "#F0F9FF";
                        textSecondary = "#7DD3FC";
                        accent = "#38BDF8";
                        gold = "#F59E0B";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#38BDF8";
                        grad2 = "#0284C7";
                        logo1 = "#7DD3FC";
                        logo2 = "#0284C7";
                        logo3 = "#0C4A6E";
                    }
                    else
                    {
                        // Glacial Sky Porcelain (Light)
                        bgPrimary = "#F0F9FF";
                        bgSecondary = "#E0F2FE";
                        bgCard = "#FFFFFF";
                        border = "#BAE6FD";
                        borderFocus = "#0284C7";
                        textPrimary = "#0C4A6E";
                        textSecondary = "#0369A1";
                        accent = "#0284C7";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#0284C7";
                        grad2 = "#0369A1";
                        logo1 = "#38BDF8";
                        logo2 = "#0284C7";
                        logo3 = "#0C4A6E";
                    }
                    break;

                case "slate":
                default:
                    if (isDark)
                    {
                        // Cinematic Dark Studio Slate (Dark)
                        bgPrimary = "#111317";
                        bgSecondary = "#171A1F";
                        bgCard = "#1E222A";
                        border = "#2B313D";
                        borderFocus = "#94A3B8";
                        textPrimary = "#F8FAFC";
                        textSecondary = "#94A3B8";
                        accent = "#F8FAFC";
                        gold = "#E5A93C";
                        danger = "#E05252";
                        success = "#38A169";
                        warning = "#E5A93C";
                        grad1 = "#F8FAFC";
                        grad2 = "#CBD5E1";
                        logo1 = "#94A3B8";
                        logo2 = "#475569";
                        logo3 = "#1E293B";
                    }
                    else
                    {
                        // Nordic Porcelain Studio (Light)
                        bgPrimary = "#F8FAFC";
                        bgSecondary = "#EDF2F7";
                        bgCard = "#FFFFFF";
                        border = "#D9E2EC";
                        borderFocus = "#475569";
                        textPrimary = "#0F172A";
                        textSecondary = "#64748B";
                        accent = "#0F172A";
                        gold = "#D97706";
                        danger = "#E11D48";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#0F172A";
                        grad2 = "#334155";
                        logo1 = "#475569";
                        logo2 = "#334155";
                        logo3 = "#0F172A";
                    }
                    break;
            }

            string btnFg = isDark ? "#0F172A" : "#FFFFFF";
            // For light mode where PrimaryAccentGradient is dark, text is white; for dark mode where PrimaryAccentGradient is bright, text is dark
            SetResourceColor(dict, "ButtonForegroundColor", btnFg);

            SetResourceColor(dict, "BgPrimaryColor", bgPrimary);
            SetResourceColor(dict, "BgSecondaryColor", bgSecondary);
            SetResourceColor(dict, "BgCardColor", bgCard);
            SetResourceColor(dict, "BorderColor", border);
            SetResourceColor(dict, "BorderFocusColor", borderFocus);
            SetResourceColor(dict, "TextPrimaryColor", textPrimary);
            SetResourceColor(dict, "TextSecondaryColor", textSecondary);
            SetResourceColor(dict, "AccentColor", accent);
            SetResourceColor(dict, "GoldColor", gold);
            SetResourceColor(dict, "DangerColor", danger);
            SetResourceColor(dict, "SuccessColor", success);
            SetResourceColor(dict, "WarningColor", warning);

            dict["PrimaryAccentGradient"] = CreateGradient(grad1, grad2);
            dict["LogoOrbGradient"] = CreateGradient(logo1, logo2, logo3);
            dict["GoldOrbGradient"] = CreateGradient("#FCD34D", "#F59E0B", "#B45309");

            // Theme-adaptive smooth ambient studio workspace gradient
            dict["MainWorkspaceGradientBrush"] = CreateWorkspaceGradient(bgPrimary, bgSecondary, bgCard, isDark);

            // Sync all SolidColorBrushes from updated Colors
            SyncBrush(dict, "BgPrimaryBrush", "BgPrimaryColor");
            SyncBrush(dict, "BgSecondaryBrush", "BgSecondaryColor");
            SyncBrush(dict, "BgCardBrush", "BgCardColor");
            SyncBrush(dict, "BorderBrush", "BorderColor");
            SyncBrush(dict, "BorderFocusBrush", "BorderFocusColor");
            SyncBrush(dict, "TextPrimaryBrush", "TextPrimaryColor");
            SyncBrush(dict, "TextSecondaryBrush", "TextSecondaryColor");
            SyncBrush(dict, "AccentBrush", "AccentColor");
            SyncBrush(dict, "ButtonForegroundBrush", "ButtonForegroundColor");
            SyncBrush(dict, "GoldBrush", "GoldColor");
            SyncBrush(dict, "DangerBrush", "DangerColor");
            SyncBrush(dict, "SuccessBrush", "SuccessColor");
            SyncBrush(dict, "WarningBrush", "WarningColor");

            // Sync Topographic Contour Brush (balanced transparency: visible yet non-distracting)
            if (dict["BorderColor"] is Color borderCol)
            {
                dict["TopoContourBrush"] = new SolidColorBrush(borderCol) { Opacity = isDark ? 0.28 : 0.24 };
            }
        }

        // Backward compatibility method
        public static void ApplyTheme(string legacyTheme)
        {
            if (string.IsNullOrWhiteSpace(legacyTheme))
            {
                ApplyTheme("Slate", true);
                return;
            }

            if (legacyTheme.Contains("_"))
            {
                var parts = legacyTheme.Split('_');
                var palette = parts[0];
                var isDark = parts.Length > 1 && string.Equals(parts[1], "Dark", StringComparison.OrdinalIgnoreCase);
                ApplyTheme(palette, isDark);
                return;
            }

            switch (legacyTheme.ToLowerInvariant())
            {
                case "light":
                    ApplyTheme("Slate", false);
                    break;
                case "emerald":
                    ApplyTheme("Emerald", true);
                    break;
                case "burgundy":
                    ApplyTheme("Burgundy", true);
                    break;
                case "amber":
                    ApplyTheme("Amber", true);
                    break;
                case "sapphire":
                    ApplyTheme("Sapphire", true);
                    break;
                case "dark":
                default:
                    ApplyTheme("Slate", true);
                    break;
            }
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

        private static LinearGradientBrush CreateWorkspaceGradient(string bgPrimary, string bgSecondary, string bgCard, bool isDark)
        {
            // Smooth, rich diagonal studio gradient respecting themes & modes
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            var col1 = (Color)ColorConverter.ConvertFromString(bgPrimary);
            var col2 = (Color)ColorConverter.ConvertFromString(bgSecondary);
            var col3 = (Color)ColorConverter.ConvertFromString(bgCard);

            brush.GradientStops.Add(new GradientStop(col1, 0.0));
            brush.GradientStops.Add(new GradientStop(col2, 0.55));
            brush.GradientStops.Add(new GradientStop(col3, 1.0));
            return brush;
        }
    }
}
