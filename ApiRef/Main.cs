using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ApiRef.Core;

namespace ApiRef
{
    public partial class Main : Form
    {
        private const string LastUse = ".last", ValueFormat = "{0};";
        private Options options;
        private ApiReference reference;

        public Main()
        {
            options = new Options() { FilterPublic = true };
            reference = new ApiReference(options);

            ReadOptions();
            InitializeComponent();

            tip.SetToolTip(txtDLL, "Local da sua biblioteca (.dll).");
            tip.SetToolTip(txtOutput, "Diretório em que os arquivos e pastas serão gerados.");
            tip.SetToolTip(txtRoot, "Diretório base para links internos.");

            cFilterPublic.Checked = options.FilterPublic;
            txtDLL.Text = options.LibraryPath;
            txtOutput.Text = options.OutputDirectory;
            txtRoot.Text = options.RootPath;
        }

        private void SaveOptions()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(ValueFormat, options.FilterPublic.ToString());
            builder.AppendFormat(ValueFormat, options.LibraryPath);
            builder.AppendFormat(ValueFormat, options.OutputDirectory);
            builder.AppendFormat(ValueFormat, options.RootPath);
            File.WriteAllText(LastUse, builder.ToString());
        }
        private void ReadOptions()
        {
            if (File.Exists(LastUse))
            {
                string[] split = File.ReadAllText(LastUse).Split(';');
                options.FilterPublic = bool.Parse(split[0]);
                options.LibraryPath = split[1];
                options.OutputDirectory = split[2];
                options.RootPath = split[3];
            }
        }
        private void UpdateOptions()
        {
            options.FilterPublic = cFilterPublic.Checked;
            options.LibraryPath = txtDLL.Text;
            options.OutputDirectory = txtOutput.Text;
            options.RootPath = txtRoot.Text;
        }

        private void btnClearOutput_Click(object sender, EventArgs e)
        {
            UpdateOptions();

            try
            {
                if (Directory.Exists(options.OutputDirectory)) Directory.Delete(options.OutputDirectory, true);

                MessageBox.Show("Diretório apagado com sucesso!");
            }
            catch
            {
                MessageBox.Show("Não foi possível apagar o diretório!");
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            UpdateOptions();
            
            if (File.Exists(options.LibraryPath) && options.OutputDirectory.Length > 0)
            {
                if (options.RootPath.Length == 0) options.RootPath = "/";

                reference.Generate();
                MessageBox.Show("Processo concluído! Verifique seu diretório.");
            }

            SaveOptions();
        }

        private void btnDLL_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Filter = "DLL | *.dll" };

            if (File.Exists(options.LibraryPath)) dialog.FileName = options.LibraryPath;

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK) txtDLL.Text = dialog.FileName;
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = options.OutputDirectory;
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK) txtOutput.Text = dialog.SelectedPath;
        }
    }
}
