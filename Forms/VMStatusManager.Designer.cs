using System.Drawing;
using System.Windows.Forms;

namespace HyperVMs
{
    partial class VMStatusManager
    {
        private NotifyIcon mainIcon;
        private ContextMenuStrip mainContextMenuStrip;

        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                mainIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // mainIcon
            // 
            this.mainIcon.ContextMenuStrip = this.mainContextMenuStrip;
            this.mainIcon.Text = "HyperVMs";
            // 
            // mainContextMenuStrip
            // 
            this.mainContextMenuStrip.Name = "mainContextMenuStrip";
            this.mainContextMenuStrip.Size = new System.Drawing.Size(181, 26);
            this.mainContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnMainContextMenuStripOpening);
            // 
            // VMStatusManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 111);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VMStatusManager";
            this.Text = "HyperVMs";
            this.ResumeLayout(false);

        }

        #endregion
    }
}

