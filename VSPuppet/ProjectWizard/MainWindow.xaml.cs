
namespace ProjectWizard
{
    using System;
    using System.Security;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string userName;

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                this.userName = value;
                tbUserName.Text = this.userName;
            }
        }

        private string moduleName;

        public string ModuleName 
        {
            get
            {
                return moduleName;
            } 

            set
            {
                this.moduleName = value;
                tbModuleName.Text = this.moduleName;
            }
        }

        public SecureString Password { get; set; }

        public string Version { get; set; }

        public string Dependency { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Version = "0.0.1";
            this.Dependency = "'puppetlabs/mysql', '1.2.3'";

            gridModuleProperties.DataContext = this;

            var userNameBinding = new Binding("UserName");
            tbUserName.SetBinding(TextBox.TextProperty, userNameBinding);

            var moduleNameBinding = new Binding("ModuleName");
            tbModuleName.SetBinding(TextBox.TextProperty, moduleNameBinding);
            
            var versionBinding = new Binding("Version");
            tbVersion.SetBinding(TextBox.TextProperty, versionBinding);

            var dependencyBinding = new Binding("Dependency");
            tbDependency.SetBinding(TextBox.TextProperty, dependencyBinding);

            var symmaryBinding = new Binding("Summary");
            tbSummary.SetBinding(TextBox.TextProperty, symmaryBinding);

            var descriptionBinding = new Binding("Description");
            tbDescriprtion.SetBinding(TextBox.TextProperty, descriptionBinding);

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            var pwdBox = sender as PasswordBox;
            if (pwdBox != null) this.Password = pwdBox.SecurePassword;
        }

    }
}
