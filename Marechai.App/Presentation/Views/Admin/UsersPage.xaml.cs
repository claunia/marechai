using System;
using System.Diagnostics;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Views.Admin;

/// <summary>
///     User management page for Uberadmins
/// </summary>
public sealed partial class UsersPage : Page
{
    private ContentDialog  _currentOpenDialog;
    private UsersViewModel _currentViewModel;

    public UsersPage()
    {
        InitializeComponent();
        Loaded             += UsersPage_Loaded;
        DataContextChanged += UsersPage_DataContextChanged;
    }

    private void UsersPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from previous ViewModel
        if(_currentViewModel != null) _currentViewModel.ShowDialogRequested -= OnShowDialogRequested;

        // Subscribe to new ViewModel
        if(DataContext is UsersViewModel vm)
        {
            _currentViewModel      =  vm;
            vm.ShowDialogRequested += OnShowDialogRequested;

            if(vm.IsUberadmin)
            {
                // Load data when DataContext is set and user is Uberadmin
                vm.LoadUsersCommand.Execute(null);
            }
        }
    }

    private void UsersPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is UsersViewModel vm && vm.IsUberadmin)
        {
            // Load data when page is loaded (fallback)
            vm.LoadUsersCommand.Execute(null);
        }
    }

    private async void OnShowDialogRequested(object sender, string dialogType)
    {
        // Close any currently open dialog first
        if(_currentOpenDialog != null)
        {
            _currentOpenDialog.Hide();
            _currentOpenDialog = null;
        }

        if(DataContext is not UsersViewModel vm) return;

        ContentDialog dialog = null;

        switch(dialogType)
        {
            case "AddEdit":
            {
                var emailBox = new TextBox
                {
                    Header = "Email",
                    Text   = vm.Email
                };

                emailBox.TextChanged += (s, e) => vm.Email = emailBox.Text;

                var userNameBox = new TextBox
                {
                    Header = "Username",
                    Text   = vm.UserName
                };

                userNameBox.TextChanged += (s, e) => vm.UserName = userNameBox.Text;

                var phoneBox = new TextBox
                {
                    Header = "Phone Number",
                    Text   = vm.PhoneNumber
                };

                phoneBox.TextChanged += (s, e) => vm.PhoneNumber = phoneBox.Text;

                var passwordBox = new PasswordBox
                {
                    Header          = "Password",
                    Password        = vm.Password,
                    PlaceholderText = "Leave blank to keep current (when editing)"
                };

                passwordBox.PasswordChanged += (s, e) => vm.Password = passwordBox.Password;

                var confirmPasswordBox = new PasswordBox
                {
                    Header   = "Confirm Password",
                    Password = vm.ConfirmPassword
                };

                confirmPasswordBox.PasswordChanged += (s, e) => vm.ConfirmPassword = confirmPasswordBox.Password;

                dialog = new ContentDialog
                {
                    XamlRoot             = XamlRoot,
                    Title                = vm.DialogTitle,
                    PrimaryButtonText    = "Save",
                    CloseButtonText      = "Cancel",
                    PrimaryButtonCommand = vm.SaveUserCommand,
                    CloseButtonCommand   = vm.CloseDialogCommand,
                    DefaultButton        = ContentDialogButton.Primary,
                    Content = new StackPanel
                    {
                        Spacing  = 12,
                        MinWidth = 400,
                        Children =
                        {
                            emailBox,
                            userNameBox,
                            phoneBox,
                            passwordBox,
                            confirmPasswordBox
                        }
                    }
                };
            }

                break;

            case "Password":
            {
                var passwordBox = new PasswordBox
                {
                    Header   = "New Password",
                    Password = vm.Password
                };

                passwordBox.PasswordChanged += (s, e) => vm.Password = passwordBox.Password;

                var confirmPasswordBox = new PasswordBox
                {
                    Header   = "Confirm Password",
                    Password = vm.ConfirmPassword
                };

                confirmPasswordBox.PasswordChanged += (s, e) => vm.ConfirmPassword = confirmPasswordBox.Password;

                dialog = new ContentDialog
                {
                    XamlRoot             = XamlRoot,
                    Title                = vm.DialogTitle,
                    PrimaryButtonText    = "Change Password",
                    CloseButtonText      = "Cancel",
                    PrimaryButtonCommand = vm.SavePasswordCommand,
                    CloseButtonCommand   = vm.CloseDialogCommand,
                    DefaultButton        = ContentDialogButton.Primary,
                    Content = new StackPanel
                    {
                        Spacing  = 12,
                        MinWidth = 400,
                        Children =
                        {
                            passwordBox,
                            confirmPasswordBox
                        }
                    }
                };
            }

                break;

            case "Roles":
            {
                Debug.WriteLine($"Creating Roles dialog. Available roles count: {vm.AvailableRoles.Count}");
                foreach(string role in vm.AvailableRoles) Debug.WriteLine($"  - Role: {role}");

                var rolesContent = new Grid
                {
                    RowSpacing = 16,
                    MinWidth   = 450
                };

                rolesContent.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });

                rolesContent.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });

                var addRolePanel = new StackPanel
                {
                    Spacing = 8
                };

                addRolePanel.Children.Add(new TextBlock
                {
                    Text       = "Add Role",
                    FontWeight = FontWeights.SemiBold
                });

                var addRoleGrid = new Grid
                {
                    ColumnSpacing = 8
                };

                addRoleGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

                addRoleGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

                // Use ListView with SingleSelection instead of ComboBox - ComboBox has issues with programmatic creation
                var roleListView = new ListView
                {
                    SelectionMode       = ListViewSelectionMode.Single,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    MaxHeight           = 200,
                    MinWidth            = 300
                };

                Debug.WriteLine($"Creating ListView for role selection with {vm.AvailableRoles.Count} roles");

                // Populate the ListView
                foreach(string role in vm.AvailableRoles)
                {
                    var item = new ListViewItem
                    {
                        Content = role
                    };

                    roleListView.Items.Add(item);
                    Debug.WriteLine($"  Added role to ListView: {role}");
                }

                Debug.WriteLine($"ListView Items.Count: {roleListView.Items.Count}");

                roleListView.SelectionChanged += (s, e) =>
                {
                    if(roleListView.SelectedItem is ListViewItem selectedItem &&
                       selectedItem.Content is string selectedRole)
                    {
                        vm.SelectedRole = selectedRole;
                        Debug.WriteLine($"Selected role from ListView: {selectedRole}");
                    }
                };

                Grid.SetColumn(roleListView, 0);
                addRoleGrid.Children.Add(roleListView);

                var addButton = new Button
                {
                    Content = "Add",
                    Command = vm.AddRoleCommand
                };

                Grid.SetColumn(addButton, 1);
                addRoleGrid.Children.Add(addButton);

                addRolePanel.Children.Add(addRoleGrid);
                Grid.SetRow(addRolePanel, 0);
                rolesContent.Children.Add(addRolePanel);

                var currentRolesPanel = new StackPanel
                {
                    Spacing = 8,
                    Margin  = new Thickness(0, 8, 0, 0)
                };

                currentRolesPanel.Children.Add(new TextBlock
                {
                    Text       = "Current Roles",
                    FontWeight = FontWeights.SemiBold
                });

                // Create a StackPanel to display roles dynamically
                var rolesStack = new StackPanel
                {
                    Spacing = 4
                };

                // Handler to refresh the roles list
                void UpdateRolesList()
                {
                    rolesStack.Children.Clear();

                    foreach(string role in vm.UserRoles)
                    {
                        var roleItem = new Grid
                        {
                            ColumnSpacing = 8,
                            Margin        = new Thickness(0, 4, 0, 4)
                        };

                        roleItem.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });

                        roleItem.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = GridLength.Auto
                        });

                        var roleText = new TextBlock
                        {
                            Text              = role,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        Grid.SetColumn(roleText, 0);
                        roleItem.Children.Add(roleText);

                        var removeButton = new Button
                        {
                            Content          = "Remove",
                            Foreground       = new SolidColorBrush(Colors.Red),
                            CommandParameter = role
                        };

                        removeButton.Click += (s, e) =>
                        {
                            var btn          = s as Button;
                            var roleToRemove = btn?.CommandParameter as string;

                            if(roleToRemove != null && vm.RemoveRoleCommand.CanExecute(roleToRemove))
                            {
                                vm.RemoveRoleCommand.Execute(roleToRemove);
                                UpdateRolesList();
                            }
                        };

                        Grid.SetColumn(removeButton, 1);
                        roleItem.Children.Add(removeButton);

                        rolesStack.Children.Add(roleItem);
                    }
                }

                // Listen to collection changes
                vm.UserRoles.CollectionChanged += (s, e) => UpdateRolesList();

                // Initial population
                UpdateRolesList();

                var scrollViewer = new ScrollViewer
                {
                    MaxHeight = 200,
                    Content   = rolesStack
                };

                currentRolesPanel.Children.Add(scrollViewer);

                Grid.SetRow(currentRolesPanel, 1);
                rolesContent.Children.Add(currentRolesPanel);

                dialog = new ContentDialog
                {
                    XamlRoot           = XamlRoot,
                    Title              = vm.DialogTitle,
                    CloseButtonText    = "Close",
                    CloseButtonCommand = vm.CloseDialogCommand,
                    DefaultButton      = ContentDialogButton.Close,
                    Content            = rolesContent
                };
            }

                break;
        }

        if(dialog != null)
        {
            _currentOpenDialog = dialog;
            await dialog.ShowAsync();
            _currentOpenDialog = null;
        }
    }
}