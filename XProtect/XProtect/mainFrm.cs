using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Resources;
using MonoFlat;
using System.Diagnostics;
namespace XProtect
{

    public partial class mainForm : Form
    {
        #region Variables
        public enum Type
        {
            @Notice,
            @Success,
            @Warning,
            @Error
        }
        public enum Type1 { Normal, Portable };
        public enum Type2 { OneFile, EachFile };
        public struct Method { public Type1 Principal; public Type2 Details;}
        Method method = new Method();
        bool saveWithOriginalName = true;

        #endregion
        public mainForm()
        {
            InitializeComponent();
            method.Principal = Type1.Normal;
            method.Details = Type2.OneFile;
        }
        private void monoFlat_Button1_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                Log("You need to add at least one file !", Type.Error);
                return;
            }
            if (txtPassword.Text == "")
            {
                Log("You need to input a password !", Type.Error);
                return;
            }
            if (method.Principal == Type1.Normal)
            {
                if (method.Details == Type2.OneFile)
                {
                    BackgroundWorker bw_normal_onefile = new BackgroundWorker();
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Any file|*.*";
                    sfd.ShowDialog();
                    if (sfd.FileName != "")
                    {
                        bw_normal_onefile.DoWork += bw_normal_onefile_DoWork;
                        bw_normal_onefile.RunWorkerAsync(sfd.FileName);

                        Log("Started encrypting ...", Type.Notice);
                    }
                }
                else
                {
                    BackgroundWorker bw_normal_eachfile = new BackgroundWorker();
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Select the folder where you want to save the encrypted files";
                    fbd.ShowDialog();
                    if (fbd.SelectedPath != "")
                    {
                        bw_normal_eachfile.DoWork += bw_normal_eachfile_DoWork;
                        bw_normal_eachfile.RunWorkerAsync(fbd.SelectedPath);
                        Log("Started encrypting ...", Type.Notice);
                    }
                }
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Application|*.exe";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    BackgroundWorker bw_portable = new BackgroundWorker();
                    bw_portable.DoWork += bw_portable_DoWork;
                    bw_portable.RunWorkerAsync(sfd.FileName);
                }

            }
        }

        void bw_portable_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableAll(true);
            string path = (string)e.Argument;
            try
            {
                List<FileHandle.FileEx> files = new List<FileHandle.FileEx>();
                int i = 1;
                foreach (string item in lstFiles.Items)
                {
                    Log(string.Format("Encrypting {0} | {1}/{2}", Path.GetFileName(item), i, lstFiles.Items.Count), Type.Notice);
                    FileHandle.FileEx file = new FileHandle.FileEx();
                    file.name = Path.GetFileName(item);
                    file.data = File.ReadAllBytes(item);
                    files.Add(file);
                    i++;
                }
                byte[] rawData = FileHandle.CombineFiles(files.ToArray());
                byte[] encryptedData = Encryption.Encrypt(rawData, txtPassword.Text);
                Log("File(s) encrypted successfully !", Type.Success);

                string ResFile = Path.Combine(Application.StartupPath, "Encrypted.resources");
                using (ResourceWriter Writer = new ResourceWriter(ResFile))
                {
                    Writer.AddResource("encfile", encryptedData);
                    Writer.Generate();
                }
                string Source = XProtect.Properties.Resources.stub;
                Compiler.CompileFromSource(Source, path, null, new string[] { ResFile });

            }
            catch
            {
                Log("Some error occured !", Type.Error);
            }
            DisableAll(false);
        }

        void bw_normal_eachfile_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableAll(true);
            Stopwatch stp = new Stopwatch();
            stp.Start();
            string path = (string)e.Argument;

                int i = 1;
                foreach (string item in lstFiles.Items)
                {
                    try
                    {
                        Log(string.Format("Encrypting {0} | {1}/{2}", Path.GetFileName(item), i, lstFiles.Items.Count), Type.Notice);
                        FileHandle.FileEx file = new FileHandle.FileEx { name = Path.GetFileName(item), data = File.ReadAllBytes(item) };
                        byte[] data = FileHandle.CombineFiles(new FileHandle.FileEx[] { file });
                        byte[] encryptedData = Encryption.Encrypt(data, txtPassword.Text);
                        File.WriteAllBytes(Path.Combine(path,Path.GetFileName(item)), encryptedData);
                        i++;
                    }
                    catch
                    {
                        Log("Some error occured at file !" + Path.GetFileName(item), Type.Error);
                        Thread.Sleep(1500);
                    }
                }
                Log("File(s) encrypted successfully !", Type.Success);
             Invoke(new MethodInvoker(delegate
            {
                MessageBox.Show(stp.ElapsedMilliseconds.ToString());
            }));
            DisableAll(false);
        }
        void bw_normal_onefile_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableAll(true);
            string path = (string)e.Argument;
            try
            {
                List<FileHandle.FileEx> files = new List<FileHandle.FileEx>();
                int i = 1;
                foreach (string item in lstFiles.Items)
                {
                    Log(string.Format("Encrypting {0} | {1}/{2}", Path.GetFileName(item), i, lstFiles.Items.Count), Type.Notice);
                    FileHandle.FileEx file = new FileHandle.FileEx();
                    file.name = Path.GetFileName(item);
                    file.data = File.ReadAllBytes(item);
                    files.Add(file);
                    i++;
                }
                byte[] rawData = FileHandle.CombineFiles(files.ToArray());
                byte[] encryptedData = Encryption.Encrypt(rawData, txtPassword.Text);
                File.WriteAllBytes(path, encryptedData);
                Log("File(s) encrypted successfully !", Type.Success);
            }
            catch
            {
                Log("Some error occured !", Type.Error);
            }
            DisableAll(false);
        }

        private void monoFlat_RadioButton2_CheckedChanged(object sender)
        {
            if (monoFlat_RadioButton2.Checked)
            {
                method.Principal = Type1.Portable;
                cboxSaveOriginal.Visible = false;
                monoFlat_RadioButton3.Enabled = monoFlat_RadioButton4.Enabled = false;
            }
            else
            {
                method.Principal = Type1.Normal;
                monoFlat_RadioButton3.Enabled = monoFlat_RadioButton4.Enabled = true;
                if (monoFlat_RadioButton4.Checked) cboxSaveOriginal.Visible = true;
            }

        }
        private void monoFlat_Button3_Click(object sender, EventArgs e)
        {
            //lstFiles.Items.Add("TesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTesTes");
            var sfd = ShowOFD("All files|*.*");
            foreach (var name in sfd.FileNames)
            {
                if (!lstFiles.Items.Contains(name))
                    lstFiles.Items.Add(name);
            }
        }
        public OpenFileDialog ShowOFD(string filter)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = filter;
            ofd.Multiselect = true;
            ofd.ShowDialog();
            return ofd;
        }
        private void monoFlat_RadioButton3_CheckedChanged(object sender)
        {
            if (monoFlat_RadioButton3.Checked) { method.Details = Type2.OneFile; cboxSaveOriginal.Visible = false; }
            else { method.Details = Type2.EachFile; cboxSaveOriginal.Visible = true; }
        }

        private void monoFlat_Button4_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex != -1)
            {
                for (int i = lstFiles.SelectedItems.Count - 1; i >= 0; i--)
                    lstFiles.Items.Remove(lstFiles.SelectedItems[i]);
            }
        }

        private void monoFlat_ThemeContainer1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void monoFlat_ThemeContainer1_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string file in FileList)
                if (!lstFiles.Items.Contains(file))
                    lstFiles.Items.Add(file);
        }

        private void lstFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                if (lstFiles.SelectedIndex != -1)
                {
                    for (int i = lstFiles.SelectedItems.Count - 1; i >= 0; i--)
                        lstFiles.Items.Remove(lstFiles.SelectedItems[i]);
                }
            }
        }
        public void Log(string text, Type type)
        {
            Invoke(new MethodInvoker(delegate
            {
                monoFlat_NotificationBox1.Text = text;
                monoFlat_NotificationBox1.NotificationType = (MonoFlat_NotificationBox.Type)type;
            }));

        }
        private void monoFlat_Button2_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                Log("You need to add at least one file !", Type.Error);
                return;
            }
            if (txtPassword.Text == "")
            {
                Log("You need to input a password !", Type.Error);
                return;
            }
            if (method.Principal == Type1.Portable)
            {
                Log("You can't use this application to decrypt files encrypted using portable method. Please use the built in decryptor !", Type.Error);
                return;
            }
            if (method.Principal == Type1.Normal)
            {
                if (method.Details == Type2.OneFile)
                {
                    BackgroundWorker bw_normal_onefile_dec = new BackgroundWorker();
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Select the folder where you want to save the decrypted files";
                    fbd.ShowDialog();
                    if (fbd.SelectedPath != "")
                    {
                        bw_normal_onefile_dec.DoWork += bw_normal_onefile_dec_DoWork;
                        bw_normal_onefile_dec.RunWorkerAsync(fbd.SelectedPath);
                        Log("Started decrypting ...", Type.Notice);
                    }
                }
                else
                {
                    BackgroundWorker bw_normal_eachfile_dec = new BackgroundWorker();
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Select the folder where you want to save the decrypted files";
                    fbd.ShowDialog();
                    if (fbd.SelectedPath != "")
                    {
                        bw_normal_eachfile_dec.DoWork += bw_normal_eachfile_dec_DoWork;
                        bw_normal_eachfile_dec.RunWorkerAsync(fbd.SelectedPath);
                        Log("Started decrypting ...", Type.Notice);
                    }
                }
            }
        }
        void bw_normal_eachfile_dec_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableAll(true);
            Stopwatch stp = new Stopwatch();
            stp.Start();
            string path = (string)e.Argument;

            int i = 1;

            foreach (string item in lstFiles.Items)
            {
                try
                {
                    Log(string.Format("Decrypting {0} | {1}/{2}", Path.GetFileName(item), i, lstFiles.Items.Count), Type.Notice);
                    byte[] data = File.ReadAllBytes(item);
                    byte[] decryptedBytes = Encryption.Decrypt(data, txtPassword.Text);
                    var files = FileHandle.SplitFiles(decryptedBytes);
                    if(files.Length == 1 && saveWithOriginalName == false)
                    {
                        File.WriteAllBytes(Path.Combine(path, Path.GetFileName(item)), files[0].data);
                    }
                    else if (files.Length == 1 && saveWithOriginalName == true)
                    {
                        File.WriteAllBytes(Path.Combine(path, files[0].name), files[0].data);
                    }
                    else
                    {
                        foreach(var file in files)
                        {
                            File.WriteAllBytes(Path.Combine(path, file.name), file.data);
                        }
                    }
                    i++;
                    Log("File(s) decrypted successfully !", Type.Success);
                }
                catch
                {
                    Log("An Error has occured at " + Path.GetFileName(item), Type.Error);
                    Thread.Sleep(1500);
                }
            }
            Log("File(s) decrypted successfully !", Type.Success);
            Invoke(new MethodInvoker(delegate
            {
                MessageBox.Show(stp.ElapsedMilliseconds.ToString());
            }));
            DisableAll(false);
        }
        void bw_normal_onefile_dec_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableAll(true);
            string path = (string)e.Argument;

            int i = 1;
            foreach (string item in lstFiles.Items)
            {
                try
                {
                    Log(string.Format("Decrypting {0} | {1}/{2}", Path.GetFileName(item), i, lstFiles.Items.Count), Type.Notice);
                    byte[] rawData = File.ReadAllBytes(item);
                    byte[] decData = Encryption.Decrypt(rawData, txtPassword.Text);
                    FileHandle.FileEx[] files = FileHandle.SplitFiles(decData);
                    int j = 1;
                    foreach (var file in files)
                    {
                        Log(string.Format("Saving {0} | {1}/{2}", file.name, j, files.Length), Type.Error);
                        File.WriteAllBytes(Path.Combine(path, file.name), file.data);
                        j++;
                    }
                    i++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Log("An Error has occured at " + Path.GetFileName(item), Type.Error);
                    Thread.Sleep(1500);
                }
                Log("File(s) decrypted successfully !", Type.Success);
            }
            DisableAll(false);
        }
        public void DisableAll(bool state)
        {
            Invoke(new MethodInvoker(delegate
            {
                monoFlat_Button1.Enabled = monoFlat_Button2.Enabled = monoFlat_Button3.Enabled = monoFlat_Button4.Enabled = monoFlat_RadioButton1.Enabled = monoFlat_RadioButton2.Enabled = monoFlat_RadioButton3.Enabled = monoFlat_RadioButton4.Enabled = lstFiles.Enabled = !state;
            }));
        }

        private void monoFlat_CheckBox1_CheckedChanged(object sender)
        {
            saveWithOriginalName = cboxSaveOriginal.Checked;
        }

        private void monoFlat_RadioButton1_CheckedChanged(object sender)
        {

        }

        private void monoFlat_ThemeContainer1_Click(object sender, EventArgs e)
        {

        }

        private void monoFlat_ThemeContainer1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
