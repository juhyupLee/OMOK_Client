namespace OMOK_Client
{
    partial class MenuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.SinglePlayBtn = new System.Windows.Forms.Button();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SinglePlayBtn
            // 
            this.SinglePlayBtn.Location = new System.Drawing.Point(218, 143);
            this.SinglePlayBtn.Name = "SinglePlayBtn";
            this.SinglePlayBtn.Size = new System.Drawing.Size(75, 23);
            this.SinglePlayBtn.TabIndex = 1;
            this.SinglePlayBtn.Text = "혼자하기";
            this.SinglePlayBtn.UseVisualStyleBackColor = true;
            this.SinglePlayBtn.Click += new System.EventHandler(this.SinglePlayBtn_Click);
            // 
            // ExitBtn
            // 
            this.ExitBtn.Location = new System.Drawing.Point(218, 208);
            this.ExitBtn.Name = "ExitBtn";
            this.ExitBtn.Size = new System.Drawing.Size(75, 23);
            this.ExitBtn.TabIndex = 2;
            this.ExitBtn.Text = "종료하기";
            this.ExitBtn.UseVisualStyleBackColor = true;
            this.ExitBtn.Click += new System.EventHandler(this.ExitBtn_Click);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 450);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.SinglePlayBtn);
            this.Name = "MenuForm";
            this.Text = "OMOK_Client";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button SinglePlayBtn;
        private System.Windows.Forms.Button ExitBtn;
    }
}

