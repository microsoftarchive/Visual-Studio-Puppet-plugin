// *********************************************************************************
// 
//     Microsoft Open Tech 
//     VSPuppet
//     Created by Vlad Shcherbakov (Akvelon)  03 2014
// 
// *********************************************************************************

using System.Security;
using System.Windows.Controls;

namespace MicrosoftOpenTech.PuppetProject
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Markup;

    public class ResourceMapper : MarkupExtension
    {
        private string ResKey { get; set; }

        public ResourceMapper(string resKey)
        {
            this.ResKey = resKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Resources.ResourceManager.GetString(this.ResKey);
        }
    }

    /// <summary>
    /// Interaction logic for ForgePublishWindow.xaml
    /// </summary>
    public partial class ForgePublishWindow : Window
    {
        public ForgePublishWindow()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(GridTop, pwdAcountPassword);
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PwdAcountPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var pwdBox = sender as PasswordBox;

            if (pwdBox != null && pwdBox.SecurePassword.Length == 0)
            {
                this.btnPublish.IsEnabled = false;
            }
            else
            {
                this.btnPublish.IsEnabled = true;
            }
        }
    }
}
