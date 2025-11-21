using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using System.Threading.Tasks;

namespace MistralCodingAssistant
{
    /// <summary>
    /// Interaction logic for MistralCodingAssistantSideTabControl.
    /// </summary>
    public partial class MistralCodingAssistantSideTabControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MistralCodingAssistantSideTabControl"/> class.
        /// </summary>
        public MistralCodingAssistantSideTabControl()
        {
            this.InitializeComponent();
        }
        private readonly HttpClient _client = new HttpClient();
        private class AIResponse
        {
            public string response {  get; set; }
        }

        // Handle chat send click button
        private async void ChatSend_Click(object sender, RoutedEventArgs e)
        {
            var prompt = ChatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
                return;

            ChatOutput.Text += $"\n\nYou: {prompt}\n";
            var payload = new
            {
                prompt = prompt,
                code = ""
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resp = await _client.PostAsync("http://127.0.0.1:8000/explain_code", content);
                var text = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<AIResponse>(text);

                ChatOutput.Text += $"\n Mistral: {parsed.response}\n";
                HistoryLog.Text += $"\n[{DateTime.Now}] {prompt}\n-> {parsed.response}\n";
            }
            catch (Exception ex)
            {
                ChatOutput.Text += $"\n [Error] {ex.Message}";
            }
        }

        // Handles explain code, optimize code, fix errors
        private string GetSelection()
        {
            try
            {
                ThreadHelper.ThrowIfOnUIThread();
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument !=  null &&
                    dte.ActiveDocument.Object("TextDocument") is TextDocument doc &&
                    doc.Selection != null)
                {
                    return doc.Selection.Text;
                }

            }
            catch (Exception e)
            {

            }
            return "";
        }

        // Gives explanation of selected code recieved from above function
        private async void ExplainSelection_Click(object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var code = GetSelection(); 
            if (string.IsNullOrWhiteSpace(code))
            {
                ChatOutput.Text += $"\n No code selected.\n";
                return;
            }
            await SendToAI("Explain this code clearly in brief: ", code);
        }

        // Optimizes code recieved from GetSelection() function
        private async void OptimizeSelection_Click(object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var code = GetSelection();
            await SendToAI("Rewrite the following C++ code for better performance and readability", code);
        }

        // Fixes errors in code if present
        private async void FixSelection_Click(object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var code = GetSelection();
            await SendToAI("Fix any errors or bugs in the code and explain what you fixed", code);
        }

        // Sneds code to FastAPI backend
        private async Task SendToAI(string prompt, string code)
        {
            ChatOutput.Text += $"\n\nYou: {prompt}\n\n{code}\n";

            var payload = new { prompt, code };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _client.PostAsync("http://127.0.0.1:8000/explain_code", content);
            var text = await resp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<AIResponse>(text);

            ChatOutput.Text += $"\nMistral: {parsed.response}\n";
        }
    }
}