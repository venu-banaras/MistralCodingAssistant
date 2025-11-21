using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace MistralCodingAssistant
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("49c2f561-9d2a-410a-810f-541bee2b5474")]
    public class MistralCodingAssistantSideTab : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MistralCodingAssistantSideTab"/> class.
        /// </summary>
        public MistralCodingAssistantSideTab() : base(null)
        {
            this.Caption = "Mistral Coding Assistant";
            this.BitmapResourceID = 301;

            this.BitmapIndex = 0;
            //object ToolWindowDockStyle = null;
            //this.ToolBarLocation = ToolWindowDockStyle.Right;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new MistralCodingAssistantSideTabControl();
        }
    }
}
