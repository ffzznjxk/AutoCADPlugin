
namespace AutoLoad
{
    partial class AutoLoad
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.checkedListBoxTools = new System.Windows.Forms.CheckedListBox();
            this.listBoxCadVersion = new System.Windows.Forms.ListBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonCancle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkedListBoxTools
            // 
            this.checkedListBoxTools.FormattingEnabled = true;
            this.checkedListBoxTools.Location = new System.Drawing.Point(12, 12);
            this.checkedListBoxTools.Name = "checkedListBoxTools";
            this.checkedListBoxTools.Size = new System.Drawing.Size(150, 184);
            this.checkedListBoxTools.TabIndex = 2;
            // 
            // listBoxCadVersion
            // 
            this.listBoxCadVersion.FormattingEnabled = true;
            this.listBoxCadVersion.ItemHeight = 17;
            this.listBoxCadVersion.Location = new System.Drawing.Point(169, 12);
            this.listBoxCadVersion.Name = "listBoxCadVersion";
            this.listBoxCadVersion.Size = new System.Drawing.Size(120, 174);
            this.listBoxCadVersion.TabIndex = 3;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(296, 13);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(100, 32);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "加载";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(295, 83);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(100, 32);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "卸载";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonCancle
            // 
            this.buttonCancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancle.Location = new System.Drawing.Point(295, 153);
            this.buttonCancle.Name = "buttonCancle";
            this.buttonCancle.Size = new System.Drawing.Size(100, 32);
            this.buttonCancle.TabIndex = 4;
            this.buttonCancle.Text = "取消";
            this.buttonCancle.UseVisualStyleBackColor = true;
            this.buttonCancle.Click += new System.EventHandler(this.buttonCancle_Click);
            // 
            // AutoLoad
            // 
            this.AcceptButton = this.buttonAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancle;
            this.ClientSize = new System.Drawing.Size(407, 199);
            this.Controls.Add(this.buttonCancle);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxCadVersion);
            this.Controls.Add(this.checkedListBoxTools);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoLoad";
            this.Text = "自动加载";
            this.Load += new System.EventHandler(this.AutoLoad_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxTools;
        private System.Windows.Forms.ListBox listBoxCadVersion;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonCancle;
    }
}

