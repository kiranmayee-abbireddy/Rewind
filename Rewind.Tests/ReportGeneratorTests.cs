using System;
using System.Collections.Generic;
using Rewind.App.Models;
using Rewind.App.Services;
using Xunit;

namespace Rewind.Tests
{
    public class ReportGeneratorTests
    {
        [Fact]
        public void GenerateMarkdownReport_ShouldFormatValidReportWithAiSummary()
        {
            var session = new Session
            {
                Title = "Feature Sprint QA",
                CaptureSource = "Primary Display",
                Status = "Completed",
                Notes = "Sprint end exploratory testing"
            };

            var bugs = new List<Bug>
            {
                new Bug
                {
                    Title = "Checkout button disabled after promo code entry",
                    ClipStartSeconds = 5.0,
                    ClipEndSeconds = 35.0,
                    ClipPath = @"C:\Rewind\sessions\s1\clips\bug1.mp4",
                    ObservedBehavior = "Checkout button turns grey and cursor shows not-allowed",
                    ExpectedBehavior = "Checkout button should remain active after coupon application",
                    ReproductionSteps = "1. Add item to cart\n2. Enter code 'SAVE20'\n3. Observe checkout button",
                    Tags = "Checkout, Billing",
                    Status = "Open",
                    AiAssisted = true,
                    AiSuggestions = "Inspect input blur validation state"
                }
            };

            var aiExecutiveSummary = "Exploratory session identified 1 blocker in the checkout checkout funnel.";

            // Act
            var report = ReportGenerator.GenerateMarkdownReport(session, bugs, "Alice Tester", aiExecutiveSummary);

            // Assert
            Assert.Contains("# QA Bug Report — Feature Sprint QA", report);
            Assert.Contains("**Tester:** Alice Tester", report);
            Assert.Contains("### 🤖 AI Executive Summary", report);
            Assert.Contains("Exploratory session identified 1 blocker", report);
            Assert.Contains("## Bug #1: Checkout button disabled after promo code entry", report);
            Assert.Contains("Observed Behavior", report);
            Assert.Contains("Expected Behavior", report);
            Assert.Contains("Steps to Reproduce", report);
            Assert.Contains("AI Assistant Note", report);
        }

        [Fact]
        public void GenerateMarkdownReport_EmptyBugs_ShouldIncludeEmptyNotice()
        {
            var session = new Session
            {
                Title = "Clean Run Session",
                Status = "Completed"
            };

            var report = ReportGenerator.GenerateMarkdownReport(session, new List<Bug>());

            Assert.Contains("No bugs were captured during this testing session.", report);
        }
    }
}
