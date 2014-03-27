// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace ProjectWizard
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Markup;
    using Res = Properties.Resources;

    public class ResourceMapper : MarkupExtension
    {
        private string ResKey { get; set; }

        public ResourceMapper(string resKey)
        {
            this.ResKey = resKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Properties.Resources.ResourceManager.GetString(this.ResKey);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public class ViewModel : System.ComponentModel.IDataErrorInfo
        {
            public string UserName { get; set; }

            public string ModuleName { get; set; }

            public string Version { get; set; }

            public string Dependency { get; set; }

            public string Summary { get; set; }

            public string Description { get; set; }

            public ViewModel()
            {
                Version = "0.0.1";
                Dependency = "'puppetlabs/stdlib', '>=2.2.1'";
            }

            public string this[string columnName]
            {
                get
                {
                    var error = string.Empty;

                    switch (columnName)
                    {
                        case "UserName":
                            if (String.IsNullOrEmpty(UserName))
                                error = string.Format(Res.IsMandatoryTempl, Res.ForgeUserName);
                            break;
                        case "ModuleName":
                            if (String.IsNullOrEmpty(ModuleName))
                                error = string.Format(Res.IsMandatoryTempl, Res.ForgeModuleName);
                            break;
                        case "Summary":
                            if (String.IsNullOrEmpty(Summary))
                                error = string.Format(Res.IsMandatoryTempl, Res.Summary);
                            break;
                        case "Version":
                        {
                            if (String.IsNullOrEmpty(Version))
                            {
                                error = string.Format(Res.IsMandatoryTempl, Res.ForgeModuleVersion);
                                break;
                            }

                            const string pattern = @"^\d+\.\d+\.\d+$";
                            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);

                            if (!rgx.IsMatch(Version))
                            {
                                error = string.Format(Res.DoNotMatchPatternTempl, Res.ForgeModuleVersion);
                            }
                            
                            break;
                        }
                        case "Dependency":
                        {
                            const string patternCom = @"^'\w+/\w+',\s*'(ge|le|g|l)?(\d+\.\d+\.(\d+|x))'$";
                            const string patternVer = @"\b((ge|le|g|l))?(?(1)\s*\d+\.\d+\.\d+|\s*\d+\.\d+\.(\d+|x))\b";
                            var dep = Dependency.Replace('<', 'l').Replace('>', 'g').Replace('=', 'e');
                            const RegexOptions opt = RegexOptions.IgnoreCase;

                            if (!new Regex(patternCom, opt).IsMatch(dep) || Regex.Matches(dep, patternVer, opt).Count == 0)
                            {
                                error = string.Format(Res.DoNotMatchPatternTempl, Res.Dependency);
                            }

                            break;
                        }
                    }

                    return error;
                }
            }

            public string Error
            {
                get { return "Please verify the form is filled in correctly"; }
            }
        }

        public readonly ViewModel viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                if (viewModel.GetType().GetProperties().Any(prop => !string.IsNullOrEmpty(viewModel[prop.Name])))
                {
                    return;
                }

                DialogResult = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
