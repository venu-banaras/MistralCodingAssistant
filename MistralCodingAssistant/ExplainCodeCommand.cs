using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Task = System.Threading.Tasks.Task;
using System.Text;
using System.Text.Json;
namespace MistralCodingAssistant

{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExplainCodeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("766815b1-2c4f-4d35-a15e-f4588250dee0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplainCodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ExplainCodeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExplainCodeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ExplainCodeCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ExplainCodeCommand(package, commandService);
        }

        private void Show(string message)
        {
            VsShellUtilities.ShowMessageBox(
              package,
              message,
              "Mistral Coding Assistant Debug",
              OLEMSGICON.OLEMSGICON_INFO,
              OLEMSGBUTTON.OLEMSGBUTTON_OK,
              OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>

        private void Execute(object sender, EventArgs e)
        {
            // We use the JoinableTaskFactory to run async calls from a sync handler safely.
            // This avoids blocking the UI thread improperly while still allowing us to await GetServiceAsync.
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // Ensure we are on the UI thread before touching DTE or any VS shell UI objects
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                // Use the async service accessor provided by AsyncPackage
                var dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
                if (dte == null)
                {
                    VsShellUtilities.ShowMessageBox(
                      this.package,
                      "DTE service not available.",
                      "Mistral Coding Assistant",
                      OLEMSGICON.OLEMSGICON_WARNING,
                      OLEMSGBUTTON.OLEMSGBUTTON_OK,
                      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                string selectedText = null;
                try
                {
                    if (dte?.ActiveDocument != null && dte.ActiveDocument.Object("TextDocument") is TextDocument textDoc)
                    {
                        var sel = textDoc.Selection;
                        selectedText = sel?.Text;
                    }
                }
                catch (Exception ex)
                {
                    Show("ERROR: " + ex.Message);
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    VsShellUtilities.ShowMessageBox(
                      this.package,
                      "Select some C++ code in the editor first, then run this command.",
                      "Mistral Coding Assistant",
                      OLEMSGICON.OLEMSGICON_INFO,
                      OLEMSGBUTTON.OLEMSGBUTTON_OK,
                      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                /*
                Below code snippet was when I was learning to create extensions for MS Visual Studio 2022 
                */
                //var display = new StringBuilder();
                //display.AppendLine("Selected code (captured by extension):");
                //display.AppendLine("--------------------------------------------------");
                //display.AppendLine(selectedText);

                //VsShellUtilities.ShowMessageBox(
                //    this.package,
                //    display.ToString(),
                //    "Mistral Coding Assistant - Selected Code",
                //    OLEMSGICON.OLEMSGICON_INFO,
                //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                // PHASE 1 -> Give explanation highlighted C++ code in an editor, using the `mistralai/Devstral-Small-2505` model
                var client = new System.Net.Http.HttpClient();

                var payload = new
                {
                    prompt = "Explain this C++ code in brief",
                    code = selectedText
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://127.0.0.1:8000/explain_code", content);
                var resultJson = await response.Content.ReadAsStringAsync();

                //Display Results on message box
                VsShellUtilities.ShowMessageBox(
                  this.package,
                  resultJson,
                  "Mistral Coding Assistant Response",
                  OLEMSGICON.OLEMSGICON_INFO,
                  OLEMSGBUTTON.OLEMSGBUTTON_OK,
                  OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            });
        }

    }
}
