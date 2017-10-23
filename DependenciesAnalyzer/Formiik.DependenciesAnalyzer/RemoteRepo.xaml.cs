using System;
using System.Windows;

namespace Formiik.DependenciesAnalyzer
{
    public partial class RemoteRepo : Window
    {
        #region Events
        public event RemoteRepoInfoDelegate RemoteRepoInfoEvent;
        #endregion

        public RemoteRepo()
        {
            InitializeComponent();

            this.TxtUrlRepo.Text = Properties.Settings.Default.RemoteRepoUrl;

            this.TxtUsuario.Text = Properties.Settings.Default.UserRemoteRepo;
        }

        private void BtnGuardarRemoteRepo_Click(object sender, RoutedEventArgs e)
        {
            var remoteRepo = this.TxtUrlRepo.Text;

            var usuario = this.TxtUsuario.Text;

            var password = this.PwdRemoteRepo.Password;

            if (!Uri.IsWellFormedUriString(remoteRepo, UriKind.Absolute))
            {
                MessageBox.Show("La url ingresada no es válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.RemoteRepoInfoEvent?.Invoke(remoteRepo, usuario, password);

                this.Close();
            }
        }
    }
}
