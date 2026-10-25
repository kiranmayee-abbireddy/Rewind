using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ellipse = System.Windows.Shapes.Ellipse;
using Rewind.App.Models;
using Rewind.App.Services;
using Rewind.App.Views;
using Xunit;

namespace Rewind.Tests
{
    public class ScreenshotGenerator
    {
        [Fact]
        public void GenerateAllScreenshots()
        {
            var outDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\docs\screenshots"));
            Directory.CreateDirectory(outDir);

            var thread = new Thread(() =>
            {
                var app = new Rewind.App.App();
                app.InitializeComponent();

                var tempDbPath = Path.Combine(Path.GetTempPath(), "RewindScreenshots_" + Guid.NewGuid().ToString("N"));
                var dbService = new DatabaseService(tempDbPath);

                // Sample test data
                var sampleSession = new Session
                {
                    Title = "Checkout Flow & Payment Validation",
                    Status = "Active",
                    Notes = "Testing checkout with multiple discount coupon scenarios, address autofill and 3D Secure payment flows.",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-42)
                };
                dbService.SaveSession(sampleSession);

                var bug1 = new Bug
                {
                    SessionId = sampleSession.Id,
                    Title = "Cart total does not reflect promo code discount upon checkout submit",
                    Status = "Open",
                    ClipStartSeconds = 12.0,
                    ClipEndSeconds = 34.5,
                    ObservedBehavior = "Applying voucher SAVE20 displays success toast, but final invoice balance retains standard price.",
                    ExpectedBehavior = "Total balance should immediately deduct 20% discount and update cart summary.",
                    ReproductionSteps = "1. Add items to cart\n2. Enter voucher code SAVE20\n3. Click Proceed to Checkout",
                    ClipPath = "checkout_promo_error.mp4",
                    AiAssisted = true,
                    AiSuggestions = "Verified visual regression on total calculation event handler; coupon payload applied but DOM total element skipped rerender."
                };
                dbService.SaveBug(bug1);

                var bug2 = new Bug
                {
                    SessionId = sampleSession.Id,
                    Title = "Payment authorization iframe triggers unresponsive script warning",
                    Status = "Open",
                    ClipStartSeconds = 5.0,
                    ClipEndSeconds = 29.0,
                    ObservedBehavior = "Clicking Pay with Credit Card opens 3D Secure frame which hangs with continuous spinner.",
                    ExpectedBehavior = "3D Secure authentication challenge should complete and return approval token within 3s.",
                    ReproductionSteps = "1. Select Credit Card\n2. Enter test OTP\n3. Click Authorize",
                    ClipPath = "3d_secure_hang.mp4"
                };
                dbService.SaveBug(bug2);

                // Create a sample mock screenshot frame to display in Video / Redaction views
                var sampleFramePath = Path.Combine(tempDbPath, "sample_frame.png");
                Directory.CreateDirectory(tempDbPath);
                CreateSampleFrameBitmap(sampleFramePath);

                // -------------------------------------------------------------
                // 1. MainWindow - Slate Theme (Default Dark Studio)
                // -------------------------------------------------------------
                var mainWinSlate = new Rewind.App.MainWindow();
                mainWinSlate.Width = 1200;
                mainWinSlate.Height = 780;
                ThemeManager.ApplyTheme("Slate", isDark: true);
                RenderWindowWithOpaqueBackground(mainWinSlate, Path.Combine(outDir, "main_studio_dark.png"), "Slate", true);

                // -------------------------------------------------------------
                // 2. MainWindow - Imperial Emerald Theme (Dark)
                // -------------------------------------------------------------
                var mainWinEmerald = new Rewind.App.MainWindow();
                mainWinEmerald.Width = 1200;
                mainWinEmerald.Height = 780;
                ThemeManager.ApplyTheme("Emerald", isDark: true);
                RenderWindowWithOpaqueBackground(mainWinEmerald, Path.Combine(outDir, "theme_emerald_dark.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 3. MainWindow - Midnight Sapphire Theme (Dark)
                // -------------------------------------------------------------
                var mainWinSapphire = new Rewind.App.MainWindow();
                mainWinSapphire.Width = 1200;
                mainWinSapphire.Height = 780;
                ThemeManager.ApplyTheme("Sapphire", isDark: true);
                RenderWindowWithOpaqueBackground(mainWinSapphire, Path.Combine(outDir, "theme_sapphire_dark.png"), "Sapphire", true);

                // -------------------------------------------------------------
                // 4. MainWindow - Royal Amethyst Theme (Dark)
                // -------------------------------------------------------------
                var mainWinAmethyst = new Rewind.App.MainWindow();
                mainWinAmethyst.Width = 1200;
                mainWinAmethyst.Height = 780;
                ThemeManager.ApplyTheme("Amethyst", isDark: true);
                RenderWindowWithOpaqueBackground(mainWinAmethyst, Path.Combine(outDir, "theme_amethyst_dark.png"), "Amethyst", true);

                // -------------------------------------------------------------
                // 5. MainWindow - Clean Studio Light Mode (Slate Light)
                // -------------------------------------------------------------
                var mainWinLight = new Rewind.App.MainWindow();
                mainWinLight.Width = 1200;
                mainWinLight.Height = 780;
                ThemeManager.ApplyTheme("Slate", isDark: false);
                RenderWindowWithOpaqueBackground(mainWinLight, Path.Combine(outDir, "theme_studio_light.png"), "Slate", false);

                // Reset back to Emerald Dark for Dialogs & Features
                ThemeManager.ApplyTheme("Emerald", isDark: true);

                // -------------------------------------------------------------
                // 6. ReportPreviewWindow (Loaded & Ready state with formatted options)
                // -------------------------------------------------------------
                var bugsList = new List<Bug> { bug1, bug2 };
                var cleanList = ReportGenerator.GenerateCleanListReport(sampleSession, bugsList);
                var md = ReportGenerator.GenerateMarkdownReport(sampleSession, bugsList);
                var jira = ReportGenerator.GenerateJiraFormat(sampleSession, bugsList);
                var json = ReportGenerator.GenerateJsonReport(sampleSession, bugsList);

                var reportWin = new ReportPreviewWindow(cleanList, md, jira, json);
                reportWin.Width = 840;
                reportWin.Height = 620;
                RenderWindowWithOpaqueBackground(reportWin, Path.Combine(outDir, "report_preview_window.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 7. SettingsDialog (10 Curated Palettes, Buffers, Hotkeys)
                // -------------------------------------------------------------
                var settings = new AppSettings
                {
                    BufferDurationSeconds = 60,
                    FrameRate = 30,
                    CaptureSource = "Entire Desktop (Primary Screen)",
                    ColorPalette = "Emerald",
                    IsDarkMode = true,
                    AiEnabled = true,
                    RecordMicrophone = false
                };
                var settingsWin = new SettingsDialog(settings);
                settingsWin.Width = 660;
                settingsWin.Height = 560;
                RenderWindowWithOpaqueBackground(settingsWin, Path.Combine(outDir, "settings_dialog.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 8. BugDetailWindow (with Frame Preview, Video Scrubber & AI Notes)
                // -------------------------------------------------------------
                bug1.ThumbnailPath = sampleFramePath;
                var bugDetailWin = new BugDetailWindow(bug1);
                bugDetailWin.Width = 850;
                bugDetailWin.Height = 640;
                RenderWindowWithOpaqueBackground(bugDetailWin, Path.Combine(outDir, "bug_detail_view.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 9. TimelineEditorDialog (Redaction Studio & Dual Sliders)
                // -------------------------------------------------------------
                var timelineWin = new TimelineEditorDialog(45.0, sampleFramePath, null);
                timelineWin.Width = 920;
                timelineWin.Height = 640;
                RenderWindowWithOpaqueBackground(timelineWin, Path.Combine(outDir, "timeline_redaction_studio.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 10. RecycleBinDialog (30-day Safe Retention & Recovery)
                // -------------------------------------------------------------
                var binWin = new RecycleBinDialog(dbService);
                binWin.Width = 740;
                binWin.Height = 480;
                RenderWindowWithOpaqueBackground(binWin, Path.Combine(outDir, "recycle_bin_dialog.png"), "Emerald", true);

                // -------------------------------------------------------------
                // 11. Floating HUD - Peeking on Screen Edge Mode (Mochi watching secretly)
                // -------------------------------------------------------------
                var hudPeeking = new FloatingHudWindow();
                // Find PeekingMascotCapsule and QuickBallCapsule
                var peekingCapsule = (Border)hudPeeking.FindName("PeekingMascotCapsule");
                var quickBallCapsule = (Grid)hudPeeking.FindName("QuickBallCapsule");
                peekingCapsule.Visibility = Visibility.Visible;
                quickBallCapsule.Visibility = Visibility.Collapsed;

                var mascotPeekRotate = (RotateTransform)hudPeeking.FindName("MascotPeekRotate");
                if (mascotPeekRotate != null) mascotPeekRotate.Angle = -32;

                RenderEdgePeekingHud(hudPeeking, Path.Combine(outDir, "floating_hud_peeking_edge.png"));

                // -------------------------------------------------------------
                // 12. Floating HUD - QuickBall Orbit Expanded Mode (Center Mascot + Satellite Orbs)
                // -------------------------------------------------------------
                var hudExpanded = new FloatingHudWindow();
                var peekingCapsule2 = (Border)hudExpanded.FindName("PeekingMascotCapsule");
                var quickBallCapsule2 = (Grid)hudExpanded.FindName("QuickBallCapsule");
                peekingCapsule2.Visibility = Visibility.Collapsed;
                quickBallCapsule2.Visibility = Visibility.Visible;

                var statusText = (TextBlock)hudExpanded.FindName("StatusText");
                var bufferTimeText = (TextBlock)hudExpanded.FindName("BufferTimeText");
                if (statusText != null) statusText.Text = "REC";
                if (bufferTimeText != null) bufferTimeText.Text = "00:48";

                RenderExpandedQuickBallHud(hudExpanded, Path.Combine(outDir, "floating_hud_expanded.png"));
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(File.Exists(Path.Combine(outDir, "main_studio_dark.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "theme_emerald_dark.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "theme_sapphire_dark.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "theme_amethyst_dark.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "theme_studio_light.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "floating_hud_peeking_edge.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "floating_hud_expanded.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "settings_dialog.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "report_preview_window.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "bug_detail_view.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "timeline_redaction_studio.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "recycle_bin_dialog.png")));
        }

        private static void RenderWindowWithOpaqueBackground(Window window, string filePath, string palette, bool isDark)
        {
            var content = (FrameworkElement)window.Content;
            window.Content = null; // Detach from window to safely measure/arrange in visual tree

            int w = (int)window.Width;
            int h = (int)window.Height;
            if (w <= 0) w = 900;
            if (h <= 0) h = 650;

            // Resolve background brush from resources or fallback
            var bgBrush = (Brush)Application.Current.FindResource("BgPrimaryBrush");
            if (bgBrush == null)
            {
                bgBrush = isDark ? new SolidColorBrush(Color.FromRgb(0x0E, 0x11, 0x17)) : new SolidColorBrush(Color.FromRgb(0xF8, 0xF9, 0xFA));
            }

            var borderBrush = (Brush)Application.Current.FindResource("BorderBrush");
            if (borderBrush == null)
            {
                borderBrush = isDark ? new SolidColorBrush(Color.FromRgb(0x28, 0x30, 0x3F)) : new SolidColorBrush(Color.FromRgb(0xDE, 0xE2, 0xE6));
            }

            // Wrap in an opaque border with subtle window border to ensure 100% solid, non-transparent PNG
            var container = new Border
            {
                Width = w,
                Height = h,
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                Child = content
            };

            content.Width = w - 2;
            content.Height = h - 2;

            container.Measure(new Size(w, h));
            container.Arrange(new Rect(0, 0, w, h));
            container.UpdateLayout();

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(container);

            SaveRtbToPng(rtb, filePath);
        }

        private static void RenderEdgePeekingHud(Window hudWindow, string filePath)
        {
            var content = (FrameworkElement)hudWindow.Content;
            hudWindow.Content = null;

            int w = 320;
            int h = 200;

            // Create desktop background mockup with right screen edge line
            var container = new Grid
            {
                Width = w,
                Height = h,
                Background = new SolidColorBrush(Color.FromRgb(0x13, 0x18, 0x22)) // Sleek dark desktop wallpaper
            };

            // Subtle desktop grid / wallpaper glow
            var wallpaperGlow = new Ellipse
            {
                Width = 260,
                Height = 260,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, -40, 0),
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0x35, 0x10, 0xB9, 0x81), 0),
                        new GradientStop(Color.FromArgb(0x00, 0x10, 0xB9, 0x81), 1)
                    }
                }
            };
            container.Children.Add(wallpaperGlow);

            // Screen edge line (Right Bezel)
            var edgeBezel = new Border
            {
                Width = 3,
                Background = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            container.Children.Add(edgeBezel);

            // Label indicating desktop right edge
            var edgeLabel = new TextBlock
            {
                Text = "◀ SCREEN BEZEL • Mochi peeking from edge",
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 16, 12)
            };
            container.Children.Add(edgeLabel);

            // Position the peeking mascot container right against the bezel (half tucked into the edge)
            content.HorizontalAlignment = HorizontalAlignment.Right;
            content.VerticalAlignment = VerticalAlignment.Center;
            content.Margin = new Thickness(0, 0, -2, 0);
            container.Children.Add(content);

            container.Measure(new Size(w, h));
            container.Arrange(new Rect(0, 0, w, h));
            container.UpdateLayout();

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(container);

            SaveRtbToPng(rtb, filePath);
        }

        private static void RenderExpandedQuickBallHud(Window hudWindow, string filePath)
        {
            var content = (FrameworkElement)hudWindow.Content;
            hudWindow.Content = null;

            int w = 260;
            int h = 240;

            // Pure, authentic desktop background showing the actual quick-ball orbit as it appears on Windows
            var container = new Grid
            {
                Width = w,
                Height = h,
                Background = new SolidColorBrush(Color.FromRgb(0x0D, 0x11, 0x1A)) // Deep desktop backdrop
            };

            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            content.Margin = new Thickness(0);
            container.Children.Add(content);

            container.Measure(new Size(w, h));
            container.Arrange(new Rect(0, 0, w, h));
            container.UpdateLayout();

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(container);

            SaveRtbToPng(rtb, filePath);
        }

        private static void CreateSampleFrameBitmap(string filePath)
        {
            int w = 640;
            int h = 360;
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // App under test window mockup
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1E, 0x24, 0x33)), null, new Rect(0, 0, w, h));
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x2D, 0x37, 0x48)), null, new Rect(0, 0, w, 32));

                // Browser / test window controls
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), null, new Point(16, 16), 5, 5);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)), null, new Point(32, 16), 5, 5);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)), null, new Point(48, 16), 5, 5);

                var ftUrl = new FormattedText("https://shop.example.com/checkout/payment",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 11,
                    new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), 96);
                dc.DrawText(ftUrl, new Point(70, 10));

                // Checkout Form
                var ftTitle = new FormattedText("ORDER SUMMARY & PAYMENT",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 16,
                    new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)), 96);
                dc.DrawText(ftTitle, new Point(40, 60));

                // Order items
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x2A, 0x33, 0x47)), null, new Rect(40, 95, 340, 140));

                var ftErr = new FormattedText("⚠ Error: Discount SAVE20 dropped upon payment submission!",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 12,
                    new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71)), 96);
                dc.DrawText(ftErr, new Point(50, 110));

                // Redacted mock box (showing active redaction area)
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00)), new Pen(new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), 1.5), new Rect(50, 160, 240, 36));

                var ftRedacted = new FormattedText("[REDACTED CREDIT CARD DATA: 4532 •••• •••• 8821]",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"), 10.5,
                    new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), 96);
                dc.DrawText(ftRedacted, new Point(56, 172));

                // Total balance box
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)), null, new Rect(420, 95, 180, 80));
                var ftTotal = new FormattedText("TOTAL: $249.99\n(Expected: $199.99)",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 13,
                    new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)), 96);
                dc.DrawText(ftTotal, new Point(435, 115));
            }

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            SaveRtbToPng(rtb, filePath);
        }

        private static void SaveRtbToPng(RenderTargetBitmap rtb, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var stream = File.Create(filePath);
            encoder.Save(stream);
        }
    }
}
