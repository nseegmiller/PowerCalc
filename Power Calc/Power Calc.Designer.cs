namespace Power_Calc
{
    partial class PowerCalc
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PowerCalc));
            this.Input = new System.Windows.Forms.TextBox();
            this.Output = new System.Windows.Forms.ListView();
            this.OutputText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // Input
            // 
            this.Input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Input.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Input.Location = new System.Drawing.Point(10, 306);
            this.Input.MaximumSize = new System.Drawing.Size(3200, 25);
            this.Input.MinimumSize = new System.Drawing.Size(4, 25);
            this.Input.Multiline = true;
            this.Input.Name = "Input";
            this.Input.Size = new System.Drawing.Size(415, 25);
            this.Input.TabIndex = 0;
            this.Input.WordWrap = false;
            this.Input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleKeys);
            this.Input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DiscardEnter);
            // 
            // Output
            // 
            this.Output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Output.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Output.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OutputText});
            this.Output.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Output.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Output.Location = new System.Drawing.Point(10, 12);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(415, 278);
            this.Output.TabIndex = 4;
            this.Output.UseCompatibleStateImageBehavior = false;
            this.Output.View = System.Windows.Forms.View.Details;
            // 
            // OutputText
            // 
            this.OutputText.Width = 411;
            // 
            // PowerCalc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 343);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.Input);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PowerCalc";
            this.Text = "Power Calc";
            this.Resize += new System.EventHandler(this.ResizeColumns);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Input;
        private System.Windows.Forms.ListView Output;
        private System.Windows.Forms.ColumnHeader OutputText;
    }
}

