namespace UnitDesigner
{
    partial class Form1
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageUnits = new System.Windows.Forms.TabPage();
            this.buttonSave = new System.Windows.Forms.Button();
            this.labelDragDrop = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUType = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxUName = new System.Windows.Forms.TextBox();
            this.tabPageBuilding = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.tabPageUnits.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageUnits);
            this.tabControl.Controls.Add(this.tabPageBuilding);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(647, 435);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageUnits
            // 
            this.tabPageUnits.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.tabPageUnits.Controls.Add(this.buttonSave);
            this.tabPageUnits.Controls.Add(this.labelDragDrop);
            this.tabPageUnits.Controls.Add(this.label2);
            this.tabPageUnits.Controls.Add(this.textBoxUType);
            this.tabPageUnits.Controls.Add(this.label1);
            this.tabPageUnits.Controls.Add(this.textBoxUName);
            this.tabPageUnits.Location = new System.Drawing.Point(4, 22);
            this.tabPageUnits.Name = "tabPageUnits";
            this.tabPageUnits.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUnits.Size = new System.Drawing.Size(639, 409);
            this.tabPageUnits.TabIndex = 0;
            this.tabPageUnits.Text = "Units";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSave.Location = new System.Drawing.Point(6, 378);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            // 
            // labelDragDrop
            // 
            this.labelDragDrop.AllowDrop = true;
            this.labelDragDrop.BackColor = System.Drawing.Color.DarkRed;
            this.labelDragDrop.Font = new System.Drawing.Font("Algerian", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDragDrop.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.labelDragDrop.Location = new System.Drawing.Point(478, 9);
            this.labelDragDrop.Name = "labelDragDrop";
            this.labelDragDrop.Size = new System.Drawing.Size(144, 109);
            this.labelDragDrop.TabIndex = 4;
            this.labelDragDrop.Text = "Drag & Drop file here to edit";
            this.labelDragDrop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelDragDrop.DragDrop += new System.Windows.Forms.DragEventHandler(this.labelDragDrop_DragDrop);
            this.labelDragDrop.DragEnter += new System.Windows.Forms.DragEventHandler(this.labelDragDrop_DragEnter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Unit Type";
            // 
            // textBoxUType
            // 
            this.textBoxUType.Location = new System.Drawing.Point(102, 32);
            this.textBoxUType.Name = "textBoxUType";
            this.textBoxUType.Size = new System.Drawing.Size(100, 20);
            this.textBoxUType.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Unit Name";
            // 
            // textBoxUName
            // 
            this.textBoxUName.Location = new System.Drawing.Point(102, 6);
            this.textBoxUName.Name = "textBoxUName";
            this.textBoxUName.Size = new System.Drawing.Size(100, 20);
            this.textBoxUName.TabIndex = 0;
            // 
            // tabPageBuilding
            // 
            this.tabPageBuilding.BackColor = System.Drawing.SystemColors.Highlight;
            this.tabPageBuilding.Location = new System.Drawing.Point(4, 22);
            this.tabPageBuilding.Name = "tabPageBuilding";
            this.tabPageBuilding.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBuilding.Size = new System.Drawing.Size(512, 397);
            this.tabPageBuilding.TabIndex = 1;
            this.tabPageBuilding.Text = "Buildings";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(647, 435);
            this.Controls.Add(this.tabControl);
            this.Name = "Form1";
            this.Text = "Unit Designer";
            this.tabControl.ResumeLayout(false);
            this.tabPageUnits.ResumeLayout(false);
            this.tabPageUnits.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageUnits;
        private System.Windows.Forms.TextBox textBoxUName;
        private System.Windows.Forms.TabPage tabPageBuilding;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label labelDragDrop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxUType;
    }
}

