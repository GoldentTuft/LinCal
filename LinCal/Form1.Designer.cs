namespace LinCal
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.sikiGrid1 = new WindowsFormsApplication1.SikiGrid();
            this.SuspendLayout();
            // 
            // sikiGrid1
            // 
            this.sikiGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sikiGrid1.BackColor = System.Drawing.SystemColors.Control;
            this.sikiGrid1.Location = new System.Drawing.Point(41, 2);
            this.sikiGrid1.MinimumSize = new System.Drawing.Size(200, 20);
            this.sikiGrid1.Name = "sikiGrid1";
            this.sikiGrid1.PadX = 30;
            this.sikiGrid1.PadY = 3;
            this.sikiGrid1.Size = new System.Drawing.Size(571, 80);
            this.sikiGrid1.TabIndex = 0;
            this.sikiGrid1.Text = "sikiGrid1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 85);
            this.ControlBox = false;
            this.Controls.Add(this.sikiGrid1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private WindowsFormsApplication1.SikiGrid sikiGrid1;
    }
}

