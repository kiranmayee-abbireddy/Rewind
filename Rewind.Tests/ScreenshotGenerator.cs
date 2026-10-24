using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            var thread = new Thread(() =>
            {
                var app = new Rewind.App.App();
                app.InitializeComponent();

                var tempDbPath = Path.Combine(Path.GetTempPath(), "RewindScreenshots_" + Guid.NewGuid().ToString("N"));
                var dbService = new DatabaseService(tempDbPath);

                var outDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\docs\screenshots"));
                Directory.CreateDirectory(outDir);

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

                // 1. MainWindow (Dark Studio with Mochi avatar, Session List, and Bug Cards)
                var mainWin = new Rewind.App.MainWindow();
                mainWin.Width = 1200;
                mainWin.Height = 780;
                RenderWindow(mainWin, Path.Combine(outDir, "main_studio_dark.png"));

                // 2. ReportPreviewWindow (Loaded & Ready state with formatted options)
                var bugsList = new List<Bug> { bug1, bug2 };
                var cleanList = ReportGenerator.GenerateCleanListReport(sampleSession, bugsList);
                var md = ReportGenerator.GenerateMarkdownReport(sampleSession, bugsList);
                var jira = ReportGenerator.GenerateJiraFormat(sampleSession, bugsList);
                var json = ReportGenerator.GenerateJsonReport(sampleSession, bugsList);

                var reportWin = new ReportPreviewWindow(cleanList, md, jira, json);
                reportWin.Width = 820;
                reportWin.Height = 620;
                RenderWindow(reportWin, Path.Combine(outDir, "report_preview_window.png"));

                // 3. SettingsDialog
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
                settingsWin.Height = 540;
                RenderWindow(settingsWin, Path.Combine(outDir, "settings_dialog.png"));

                // 4. BugDetailWindow
                var bugDetailWin = new BugDetailWindow(bug1);
                bugDetailWin.Width = 820;
                bugDetailWin.Height = 640;
                RenderWindow(bugDetailWin, Path.Combine(outDir, "bug_detail_view.png"));

                // 5. TimelineEditorDialog (Redaction Studio)
                var timelineWin = new TimelineEditorDialog(45.0, null, null);
                timelineWin.Width = 880;
                timelineWin.Height = 580;
                RenderWindow(timelineWin, Path.Combine(outDir, "timeline_redaction_studio.png"));

                // 6. FloatingHudWindow
                var hudWin = new FloatingHudWindow();
                hudWin.Width = 110;
                hudWin.Height = 220;
                RenderWindow(hudWin, Path.Combine(outDir, "floating_hud_overlay.png"));

                // 7. RecycleBinDialog
                var binWin = new RecycleBinDialog(dbService);
                binWin.Width = 720;
                binWin.Height = 480;
                RenderWindow(binWin, Path.Combine(outDir, "recycle_bin_dialog.png"));
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\main_studio_dark.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\report_preview_window.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\settings_dialog.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\bug_detail_view.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\timeline_redaction_studio.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\floating_hud_overlay.png"));
            Assert.True(File.Exists(@"d:\projects\Rewind\docs\screenshots\recycle_bin_dialog.png"));
        }

        private static void RenderWindow(Window window, string filePath)
        {
            var content = (FrameworkElement)window.Content;
            window.Content = null; // Detach from window to render purely in visual tree

            int w = (int)window.Width;
            int h = (int)window.Height;
            if (w <= 0) w = 800;
            if (h <= 0) h = 600;

            content.Width = w;
            content.Height = h;
            content.Measure(new Size(w, h));
            content.Arrange(new Rect(0, 0, w, h));
            content.UpdateLayout();

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(content);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stream = File.Create(filePath))
            {
                encoder.Save(stream);
            }
        }
    }
}
