using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.Windows.Controls;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using EnvDTE80;
using Newtonsoft.Json;

namespace VidvatAIAssistant
{

    [Guid("586f0ccc-4a45-40a2-a87c-58d0ef3c43c5")]
    public class VidvatToolWindow : ToolWindowPane
    {
        private VidvatWebViewHost hostControl;
        public VidvatToolWindow() : base(null)
        {
            this.Caption = "Vidvat";
            hostControl = new VidvatWebViewHost(this);
            this.Content = hostControl;
        }

        public async Task SendSelectedTextToWebViewAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var dte = (EnvDTE80.DTE2)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE));
                string selectedText = "";
                if (dte?.ActiveDocument != null)
                {
                    var sel = dte.ActiveDocument.Selection as EnvDTE.TextSelection;
                    selectedText = sel?.Text ?? "";
                }

                var payload = new { type = "Selection", text = selectedText };
                var json = JsonConvert.SerializeObject(payload);
                hostControl.PostMessage(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendSelectedTextToWebViewAsync error: " + ex.ToString());
            }
        }
    }
}
