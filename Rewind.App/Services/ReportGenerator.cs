using System;
using System.Collections.Generic;
using System.Text;
using Rewind.App.Models;

namespace Rewind.App.Services
{
    public class ReportGenerator
    {
        public static string GenerateMarkdownReport(Session session, List<Bug> bugs, string testerName = "Tester")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# QA Bug Report — {session.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Date:** {session.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine($"**Tester:** {testerName}");
            sb.AppendLine($"**Session Status:** {session.Status}");
            sb.AppendLine($"**Capture Source:** {session.CaptureSource}");
            sb.AppendLine($"**Total Preserved Bugs:** {bugs.Count}");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(session.Notes))
            {
                sb.AppendLine("### Session Overview");
                sb.AppendLine(session.Notes);
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();

            if (bugs.Count == 0)
            {
                sb.AppendLine("*No bugs were captured during this testing session.*");
                return sb.ToString();
            }

            for (int i = 0; i < bugs.Count; i++)
            {
                var bug = bugs[i];
                sb.AppendLine($"## Bug #{i + 1}: {bug.Title}");
                sb.AppendLine();
                sb.AppendLine($"- **Timestamp:** {bug.CreatedAt:HH:mm:ss}");
                sb.AppendLine($"- **Status:** `{bug.Status}`");
                sb.AppendLine($"- **Clip Duration:** {bug.DurationSeconds:0.0}s ({bug.ClipStartSeconds:0.0}s → {bug.ClipEndSeconds:0.0}s)");
                sb.AppendLine($"- **Evidence Video:** `{System.IO.Path.GetFileName(bug.ClipPath)}`");
                if (!string.IsNullOrWhiteSpace(bug.Tags))
                {
                    sb.AppendLine($"- **Tags:** {bug.Tags}");
                }
                sb.AppendLine();

                sb.AppendLine("### Observed Behavior");
                sb.AppendLine(string.IsNullOrWhiteSpace(bug.ObservedBehavior) ? "_Unspecified by tester_" : bug.ObservedBehavior);
                sb.AppendLine();

                sb.AppendLine("### Expected Behavior");
                sb.AppendLine(string.IsNullOrWhiteSpace(bug.ExpectedBehavior) ? "_Unspecified by tester_" : bug.ExpectedBehavior);
                sb.AppendLine();

                sb.AppendLine("### Steps to Reproduce");
                if (string.IsNullOrWhiteSpace(bug.ReproductionSteps))
                {
                    sb.AppendLine("1. _Tester has not yet filled manual reproduction steps._");
                }
                else
                {
                    sb.AppendLine(bug.ReproductionSteps);
                }
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(bug.Notes))
                {
                    sb.AppendLine("### Tester Notes");
                    sb.AppendLine(bug.Notes);
                    sb.AppendLine();
                }

                if (bug.AiAssisted && !string.IsNullOrWhiteSpace(bug.AiSuggestions))
                {
                    sb.AppendLine("> **AI Assistant Note:** The following suggestions were drafted from screen evidence and should be reviewed by the tester:");
                    sb.AppendLine($"> {bug.AiSuggestions.Replace("\n", "\n> ")}");
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            sb.AppendLine("*Generated automatically by [REWIND](https://github.com/kiranmayee-abbireddy/Rewind) — Local-First Screen Evidence & Bug Capture for QA.*");

            return sb.ToString();
        }
    }
}
