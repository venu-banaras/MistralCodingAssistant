using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace VidvatAIAssistant
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VidvatAIAssistantPackage.PackageGuids.PackageGuidString)]
    [InstalledProductRegistration("VidvatAI", "Vidvat - Open source AI powered assistant for Visual Studio", "0.1")]
    [ProvideToolWindow(typeof(VidvatToolWindow), Style = VsDockStyle.Tabbed, Orientation = (ToolWindowOrientation)2)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(VidvatToolWindow))]
    public sealed class VidvatAIAssistantPackage : AsyncPackage
    {
        internal static class PackageGuids
        {
            public const string PackageGuidString = "4cee904e-cf10-4b2e-8e78-dc1cb10faeef";
            public const string CommandSetString = "{855481C4-4CCE-4965-9A9E-3E82382D48CE}";
        }


        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await VidvatCommand.InitializeAsync(this);
            await VidvatCommand.InitializeAsync(this);
            await VidvatToolWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
