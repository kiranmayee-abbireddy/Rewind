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
                        // Clean Studio Sage Porcelain (Light) - Calming, low saturation, professional
                        bgPrimary = "#F9FAF9";
                        bgSecondary = "#F0F4F1";
                        bgCard = "#FFFFFF";
                        border = "#DCE4DE";
                        borderFocus = "#059669";
                        textPrimary = "#111814";
                        textSecondary = "#4B5E52";
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
                        // Clean Studio Rose Porcelain (Light) - Elegant, soft neutral warmth
                        bgPrimary = "#FAF9F9";
                        bgSecondary = "#F4F0F1";
                        bgCard = "#FFFFFF";
                        border = "#E6DEDF";
                        borderFocus = "#E11D48";
                        textPrimary = "#1A1013";
                        textSecondary = "#5C4A50";
                        accent = "#BE123C";
                        gold = "#D97706";
                        danger = "#BE123C";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#BE123C";
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
                        // Clean Studio Warm Sand Porcelain (Light) - Grounded, non-flashy parchment
                        bgPrimary = "#FAF9F6";
                        bgSecondary = "#F3F1EC";
                        bgCard = "#FFFFFF";
                        border = "#E5E1D8";
                        borderFocus = "#D97706";
                        textPrimary = "#1C1710";
                        textSecondary = "#5E5345";
                        accent = "#B45309";
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
                        // Clean Studio Arctic Slate Porcelain (Light) - Clean, calm corporate tech
                        bgPrimary = "#F8FAFC";
                        bgSecondary = "#F0F3F7";
                        bgCard = "#FFFFFF";
                        border = "#DCE2E9";
                        borderFocus = "#0284C7";
                        textPrimary = "#0F172A";
                        textSecondary = "#475569";
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

                case "midnight":
                    if (isDark)
                    {
                        // True OLED Deep Space (Dark)
                        bgPrimary = "#030712";
                        bgSecondary = "#0B0F1A";
                        bgCard = "#111827";
                        border = "#1F2937";
                        borderFocus = "#6366F1";
                        textPrimary = "#F9FAFB";
                        textSecondary = "#9CA3AF";
                        accent = "#6366F1";
                        gold = "#F59E0B";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#6366F1";
                        grad2 = "#4F46E5";
                        logo1 = "#818CF8";
                        logo2 = "#6366F1";
                        logo3 = "#312E81";
                    }
                    else
                    {
                        // Clean Deep Lavender Porcelain (Light)
                        bgPrimary = "#F8F9FA";
                        bgSecondary = "#F1F3F9";
                        bgCard = "#FFFFFF";
                        border = "#DDE2EC";
                        borderFocus = "#4F46E5";
                        textPrimary = "#0F172A";
                        textSecondary = "#475569";
                        accent = "#4F46E5";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#4F46E5";
                        grad2 = "#3730A3";
                        logo1 = "#6366F1";
                        logo2 = "#4F46E5";
                        logo3 = "#312E81";
                    }
                    break;

                case "monokai":
                    if (isDark)
                    {
                        // Monokai Pro Studio Charcoal (Dark)
                        bgPrimary = "#19181A";
                        bgSecondary = "#221F22";
                        bgCard = "#2D2A2E";
                        border = "#403E41";
                        borderFocus = "#FFD866";
                        textPrimary = "#FCFCFA";
                        textSecondary = "#939293";
                        accent = "#FF6188";
                        gold = "#FFD866";
                        danger = "#FF6188";
                        success = "#A9DC76";
                        warning = "#FC9867";
                        grad1 = "#FF6188";
                        grad2 = "#FC9867";
                        logo1 = "#FFD866";
                        logo2 = "#FF6188";
                        logo3 = "#782236";
                    }
                    else
                    {
                        // Monokai Warm Studio Linen (Light)
                        bgPrimary = "#FAF9F6";
                        bgSecondary = "#F2EFE9";
                        bgCard = "#FFFFFF";
                        border = "#E2DDD3";
                        borderFocus = "#D97706";
                        textPrimary = "#1E1C1A";
                        textSecondary = "#5E5A54";
                        accent = "#D946EF";
                        gold = "#B45309";
                        danger = "#E11D48";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#C026D3";
                        grad2 = "#9333EA";
                        logo1 = "#E879F9";
                        logo2 = "#C026D3";
                        logo3 = "#581C87";
                    }
                    break;

                case "nord":
                    if (isDark)
                    {
                        // Arctic Polar Night (Dark)
                        bgPrimary = "#242933";
                        bgSecondary = "#2E3440";
                        bgCard = "#3B4252";
                        border = "#434C5E";
                        borderFocus = "#88C0D0";
                        textPrimary = "#ECEFF4";
                        textSecondary = "#D8DEE9";
                        accent = "#88C0D0";
                        gold = "#EBCB8B";
                        danger = "#BF616A";
                        success = "#A3BE8C";
                        warning = "#D08770";
                        grad1 = "#88C0D0";
                        grad2 = "#81A1C1";
                        logo1 = "#8FBCBB";
                        logo2 = "#88C0D0";
                        logo3 = "#5E81AC";
                    }
                    else
                    {
                        // Snow Storm Glacier (Light)
                        bgPrimary = "#F8FAFC";
                        bgSecondary = "#F1F5F9";
                        bgCard = "#FFFFFF";
                        border = "#CBD5E1";
                        borderFocus = "#0284C7";
                        textPrimary = "#1E293B";
                        textSecondary = "#475569";
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

                case "cyberpunk":
                    if (isDark)
                    {
                        // Night City Neon Noir (Dark)
                        bgPrimary = "#0D0A14";
                        bgSecondary = "#151022";
                        bgCard = "#1D1630";
                        border = "#2E224D";
                        borderFocus = "#06B6D4";
                        textPrimary = "#FDF4FF";
                        textSecondary = "#C084FC";
                        accent = "#A855F7";
                        gold = "#FBBF24";
                        danger = "#F43F5E";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#A855F7";
                        grad2 = "#06B6D4";
                        logo1 = "#E879F9";
                        logo2 = "#A855F7";
                        logo3 = "#3B0764";
                    }
                    else
                    {
                        // High-Tech Clean Violet Porcelain (Light)
                        bgPrimary = "#FAF9FD";
                        bgSecondary = "#F3F0F9";
                        bgCard = "#FFFFFF";
                        border = "#E3DEF0";
                        borderFocus = "#7C3AED";
                        textPrimary = "#140F24";
                        textSecondary = "#52446D";
                        accent = "#7C3AED";
                        gold = "#D97706";
                        danger = "#E11D48";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#7C3AED";
                        grad2 = "#4F46E5";
                        logo1 = "#A78BFA";
                        logo2 = "#7C3AED";
                        logo3 = "#2E1065";
                    }
                    break;

                case "forest":
                    if (isDark)
                    {
                        // Ancient Woodland Pine (Dark)
                        bgPrimary = "#0A0F0D";
                        bgSecondary = "#111A16";
                        bgCard = "#18241F";
                        border = "#253830";
                        borderFocus = "#10B981";
                        textPrimary = "#F0FAF4";
                        textSecondary = "#86A896";
                        accent = "#10B981";
                        gold = "#E5A93C";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#E5A93C";
                        grad1 = "#10B981";
                        grad2 = "#059669";
                        logo1 = "#6EE7B7";
                        logo2 = "#10B981";
                        logo3 = "#064E3B";
                    }
                    else
                    {
                        // Muted Earth Moss Porcelain (Light)
                        bgPrimary = "#F9FAF9";
                        bgSecondary = "#F1F4F1";
                        bgCard = "#FFFFFF";
                        border = "#DBE3DC";
                        borderFocus = "#059669";
                        textPrimary = "#111913";
                        textSecondary = "#435948";
                        accent = "#059669";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#059669";
                        grad2 = "#047857";
                        logo1 = "#34D399";
                        logo2 = "#059669";
                        logo3 = "#064E3B";
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
