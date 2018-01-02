using System;
using System.Windows;
using System.Windows.Forms;

namespace Formiik.DependenciesAnalyzer
{
    public partial class RemoteRepo
    {
        #region Events
        public event RemoteRepoInfoDelegate RemoteRepoInfoEvent;
        #endregion

        public RemoteRepo()
        {
            InitializeComponent();

            this.TxtUrlRepo.Text = Properties.Settings.Default.RemoteRepoUrl;

            this.TxtUsuario.Text = Properties.Settings.Default.UserRemoteRepo;

            this.TxtPathGit.Text = Properties.Settings.Default.PathGit;

            this.PwdRemoteRepo.Password = Properties.Settings.Default.PasswordRemoteRepo;
        }

        private void BtnGuardarRemoteRepo_Click(object sender, RoutedEventArgs e)
        {
            var remoteRepo = this.TxtUrlRepo.Text;

            var usuario = this.TxtUsuario.Text;

            var password = this.PwdRemoteRepo.Password;

            var pathGit = this.TxtPathGit.Text;

            if (!Uri.IsWellFormedUriString(remoteRepo, UriKind.Absolute))
            {
                System.Windows.MessageBox.Show("La url ingresada no es válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.RemoteRepoInfoEvent?.Invoke(remoteRepo, usuario, password, pathGit);

                this.Close();
            }
        }

        private void BtnSelectPathGit_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = @"Git Executable|*.exe",
                Title = @"Select Git Executable"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.TxtPathGit.Text = openFileDialog.FileName;
            }
        }
    }
}
