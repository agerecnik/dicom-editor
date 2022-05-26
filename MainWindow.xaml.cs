﻿using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DicomEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>();
        }

        private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentView.Opacity = 1;
        }

        private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentView.Opacity = 0.5;
        }

        private void ViewPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuToggleButton.IsChecked = false;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuRadioButton_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility
            if (MenuToggleButton.IsChecked == true)
            {
                ImportToolTip.Visibility = Visibility.Collapsed;
                EditorToolTip.Visibility = Visibility.Collapsed;
                SettingsToolTip.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImportToolTip.Visibility = Visibility.Visible;
                EditorToolTip.Visibility = Visibility.Visible;
                SettingsToolTip.Visibility = Visibility.Visible;
            }
        }
    }
}
