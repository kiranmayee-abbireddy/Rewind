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

                case "copper":
                    if (isDark)
                    {
                        // Industrial Copper & Raw Rust Bronze (Dark)
                        bgPrimary = "#120D0A";
                        bgSecondary = "#1A130E";
                        bgCard = "#241B14";
                        border = "#3D2B1F";
                        borderFocus = "#EA580C";
                        textPrimary = "#FFEDD5";
                        textSecondary = "#FB923C";
                        accent = "#EA580C";
                        gold = "#F59E0B";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F97316";
                        grad1 = "#EA580C";
                        grad2 = "#C2410C";
                        logo1 = "#FDBA74";
                        logo2 = "#EA580C";
                        logo3 = "#7C2D12";
                    }
                    else
                    {
                        // Clean Terracotta & Clay Porcelain (Light)
                        bgPrimary = "#FAF9F7";
                        bgSecondary = "#F4F0EB";
                        bgCard = "#FFFFFF";
                        border = "#E6DDD3";
                        borderFocus = "#C2410C";
                        textPrimary = "#1C140E";
                        textSecondary = "#5C493D";
                        accent = "#C2410C";
                        gold = "#B45309";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#EA580C";
                        grad1 = "#C2410C";
                        grad2 = "#9A3412";
                        logo1 = "#EA580C";
                        logo2 = "#C2410C";
                        logo3 = "#7C2D12";
                    }
                    break;

                case "amethyst":
                    if (isDark)
                    {
                        // Imperial Amethyst & Midnight Violet (Dark)
                        bgPrimary = "#0E0916";
                        bgSecondary = "#150E22";
                        bgCard = "#1D1330";
                        border = "#301F52";
                        borderFocus = "#A855F7";
                        textPrimary = "#FAF5FF";
                        textSecondary = "#C084FC";
                        accent = "#A855F7";
                        gold = "#FBBF24";
                        danger = "#F43F5E";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#A855F7";
                        grad2 = "#7E22CE";
                        logo1 = "#D8B4FE";
                        logo2 = "#A855F7";
                        logo3 = "#581C87";
                    }
                    else
                    {
                        // Clean Lavender Quartz Porcelain (Light)
                        bgPrimary = "#FAF9FD";
                        bgSecondary = "#F3EFF9";
                        bgCard = "#FFFFFF";
                        border = "#E2DAEE";
                        borderFocus = "#7E22CE";
                        textPrimary = "#170F26";
                        textSecondary = "#55446D";
                        accent = "#7E22CE";
                        gold = "#D97706";
                        danger = "#E11D48";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#7E22CE";
                        grad2 = "#6B21A8";
                        logo1 = "#A855F7";
                        logo2 = "#7E22CE";
                        logo3 = "#3B0764";
                    }
                    break;

                case "ocean":
                    if (isDark)
                    {
                        // Deep Abyssal Teal & Marine Cyan (Dark)
                        bgPrimary = "#060F12";
                        bgSecondary = "#0A171C";
                        bgCard = "#0F2128";
                        border = "#183642";
                        borderFocus = "#06B6D4";
                        textPrimary = "#ECFEFF";
                        textSecondary = "#67E8F9";
                        accent = "#06B6D4";
                        gold = "#F59E0B";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#06B6D4";
                        grad2 = "#0891B2";
                        logo1 = "#67E8F9";
                        logo2 = "#06B6D4";
                        logo3 = "#164E63";
                    }
                    else
                    {
                        // Clean Marine Teal Porcelain (Light)
                        bgPrimary = "#F8FAFB";
                        bgSecondary = "#EEF5F7";
                        bgCard = "#FFFFFF";
                        border = "#D7E6EA";
                        borderFocus = "#0891B2";
                        textPrimary = "#081A20";
                        textSecondary = "#395863";
                        accent = "#0891B2";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#0891B2";
                        grad2 = "#0E7490";
                        logo1 = "#06B6D4";
                        logo2 = "#0891B2";
                        logo3 = "#155E75";
                    }
                    break;

                case "solar":
                    if (isDark)
                    {
                        // Solar Flare & Electric Gold Cyber (Dark)
                        bgPrimary = "#120E04";
                        bgSecondary = "#1A1406";
                        bgCard = "#241C0A";
                        border = "#3D2E0F";
                        borderFocus = "#EAB308";
                        textPrimary = "#FEFCE8";
                        textSecondary = "#FACC15";
                        accent = "#EAB308";
                        gold = "#FACC15";
                        danger = "#EF4444";
                        success = "#10B981";
                        warning = "#F59E0B";
                        grad1 = "#EAB308";
                        grad2 = "#CA8A04";
                        logo1 = "#FEF08A";
                        logo2 = "#EAB308";
                        logo3 = "#713F12";
                    }
                    else
                    {
                        // Clean Champagne Ochre Porcelain (Light)
                        bgPrimary = "#FAF9F5";
                        bgSecondary = "#F4F1E7";
                        bgCard = "#FFFFFF";
                        border = "#E8E3D2";
                        borderFocus = "#A16207";
                        textPrimary = "#1C1705";
                        textSecondary = "#5D5333";
                        accent = "#A16207";
                        gold = "#CA8A04";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#CA8A04";
                        grad1 = "#A16207";
                        grad2 = "#854D0E";
                        logo1 = "#CA8A04";
                        logo2 = "#A16207";
                        logo3 = "#713F12";
                    }
                    break;

                case "sage":
                    if (isDark)
                    {
                        // Nordic Lichen & Eucalyptus Sage (Dark)
                        bgPrimary = "#0D110F";
                        bgSecondary = "#131916";
                        bgCard = "#1A221E";
                        border = "#2A3730";
                        borderFocus = "#84CC16";
                        textPrimary = "#F7FEE7";
                        textSecondary = "#A3E635";
                        accent = "#84CC16";
                        gold = "#E5A93C";
                        danger = "#EF4444";
                        success = "#84CC16";
                        warning = "#E5A93C";
                        grad1 = "#84CC16";
                        grad2 = "#65A30D";
                        logo1 = "#BEF264";
                        logo2 = "#84CC16";
                        logo3 = "#365314";
                    }
                    else
                    {
                        // Clean Botanical Herb Porcelain (Light)
                        bgPrimary = "#FAFBF9";
                        bgSecondary = "#F1F5EE";
                        bgCard = "#FFFFFF";
                        border = "#DCE4D6";
                        borderFocus = "#65A30D";
                        textPrimary = "#121A0F";
                        textSecondary = "#465741";
                        accent = "#4D7C0F";
                        gold = "#D97706";
                        danger = "#DC2626";
                        success = "#16A34A";
                        warning = "#D97706";
                        grad1 = "#4D7C0F";
                        grad2 = "#3F6212";
                        logo1 = "#65A30D";
                        logo2 = "#4D7C0F";
                        logo3 = "#1A2E05";
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
