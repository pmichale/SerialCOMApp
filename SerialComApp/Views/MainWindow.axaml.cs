using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SerialComApp.ViewModels;

namespace SerialComApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ScrollViewer _scrollViewer;

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new MainWindowViewModel();

            //var textBox = this.FindControl<TextBox>("ReceivedTextBox");
            //_scrollViewer = this.FindControl<ScrollViewer>("LogSV");

            //textBox.PropertyChanged += TextBoxPropertyChanged;
            //to implement autoscroll:
            //https://github.com/AvaloniaUI/Avalonia/discussions/12931
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string selectedText = comboBox.SelectedItem.ToString();
                string comboBoxName = comboBox.Name;

                if (DataContext is MainWindowViewModel viewModel)
                {
                    switch (comboBoxName)
                    {
                        case "DataBitsCombo":
                            viewModel.SelectedDataBitsItem = selectedText;
                            break;
                        case "ParityCombo":
                            viewModel.SelectedParityItem = selectedText;
                            break;
                        case "StopBitsCombo":
                            viewModel.SelectedStopBitsItem = selectedText;
                            break;
                        case "FlowControlCombo":
                            viewModel.SelectedFlowControlItem = selectedText;
                            break;
                        case "EncodingCombo":
                            viewModel.SelectedEncodingItem = selectedText;
                            break;
                    }
                }
            }
        }

        private void TextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            // Check if the "Text" property has changed
            if (e.Property.Name == "Text")
            {
                // Scroll to the end of the ScrollViewer
                _scrollViewer.ScrollToEnd();
            }
        }

        private async void SaveConfigClicked()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open File";
            dialog.Filters.Add(new FileDialogFilter { Name = "All Files", Extensions = { "*" } });

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length > 0)
            {
                string filePath = result[0];
                // Use filePath as needed
            }
        }
    }
}