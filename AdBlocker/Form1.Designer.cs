namespace AddBlocker
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblBlocked;
        private System.Windows.Forms.Label lblAllowed;
        private System.Windows.Forms.ListBox listLog;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Button btnTheme;
        private System.Windows.Forms.Label lblCustom;
        private System.Windows.Forms.TextBox txtCustomDomain;
        private System.Windows.Forms.Button btnAddDomain;
        private System.Windows.Forms.Button btnEaster;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblBlocked = new System.Windows.Forms.Label();
            this.lblAllowed = new System.Windows.Forms.Label();
            this.listLog = new System.Windows.Forms.ListBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.btnTheme = new System.Windows.Forms.Button();
            this.lblCustom = new System.Windows.Forms.Label();
            this.txtCustomDomain = new System.Windows.Forms.TextBox();
            this.btnAddDomain = new System.Windows.Forms.Button();
            this.btnEaster = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(55, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(80, 20);
            this.txtPort.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(150, 10);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(90, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(250, 10);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(90, 23);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(350, 10);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(90, 23);
            this.btnUpdate.TabIndex = 4;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 45);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(12, 70);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(43, 13);
            this.lblTotal.TabIndex = 6;
            this.lblTotal.Text = "Total: 0";
            // 
            // lblBlocked
            // 
            this.lblBlocked.AutoSize = true;
            this.lblBlocked.Location = new System.Drawing.Point(120, 70);
            this.lblBlocked.Name = "lblBlocked";
            this.lblBlocked.Size = new System.Drawing.Size(58, 13);
            this.lblBlocked.TabIndex = 7;
            this.lblBlocked.Text = "Blocked: 0";
            // 
            // lblAllowed
            // 
            this.lblAllowed.AutoSize = true;
            this.lblAllowed.Location = new System.Drawing.Point(240, 70);
            this.lblAllowed.Name = "lblAllowed";
            this.lblAllowed.Size = new System.Drawing.Size(56, 13);
            this.lblAllowed.TabIndex = 8;
            this.lblAllowed.Text = "Allowed: 0";
            // 
            // listLog
            // 
            this.listLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLog.FormattingEnabled = true;
            this.listLog.HorizontalScrollbar = true;
            this.listLog.Location = new System.Drawing.Point(12, 95);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(776, 342);
            this.listLog.TabIndex = 9;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(12, 15);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(26, 13);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Port";
            // 
            // btnTheme
            // 
            this.btnTheme.Location = new System.Drawing.Point(750, 10);
            this.btnTheme.Name = "btnTheme";
            this.btnTheme.Size = new System.Drawing.Size(38, 23);
            this.btnTheme.TabIndex = 10;
            this.btnTheme.Text = "◐";
            this.btnTheme.UseVisualStyleBackColor = true;
            this.btnTheme.Click += new System.EventHandler(this.btnTheme_Click);
            this.lblCustom.AutoSize = true;
            this.lblCustom.Location = new System.Drawing.Point(12, 45);
            this.lblCustom.Name = "lblCustom";
            this.lblCustom.Size = new System.Drawing.Size(78, 13);
            this.lblCustom.TabIndex = 14;
            this.lblCustom.Text = "Custom block";

            this.txtCustomDomain.Location = new System.Drawing.Point(100, 42);
            this.txtCustomDomain.Name = "txtCustomDomain";
            this.txtCustomDomain.Size = new System.Drawing.Size(240, 20);
            this.txtCustomDomain.TabIndex = 15;

            this.btnAddDomain.Location = new System.Drawing.Point(350, 40);
            this.btnAddDomain.Name = "btnAddDomain";
            this.btnAddDomain.Size = new System.Drawing.Size(90, 23);
            this.btnAddDomain.TabIndex = 16;
            this.btnAddDomain.Text = "Add";
            this.btnAddDomain.UseVisualStyleBackColor = true;
            this.btnAddDomain.Click += new System.EventHandler(this.btnAddDomain_Click);
            // 
            // btnEaster
            // 
            this.btnEaster.Location = new System.Drawing.Point(750, 35);
            this.btnEaster.Name = "btnEaster";
            this.btnEaster.Size = new System.Drawing.Size(1, 1);
            this.btnEaster.TabIndex = 17;
            this.btnEaster.UseVisualStyleBackColor = true;
            this.btnEaster.Click += new System.EventHandler(this.btnEaster_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.lblAllowed);
            this.Controls.Add(this.lblBlocked);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.btnTheme);
            this.Controls.Add(this.lblCustom);
            this.Controls.Add(this.txtCustomDomain);
            this.Controls.Add(this.btnAddDomain);
            this.Controls.Add(this.btnEaster);
            this.Name = "Form1";
            this.Text = "SchoolAdBlocker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

