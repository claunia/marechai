/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

#nullable enable

using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class MachineViewPage : Page
{
    private object? _navigationSource;
    private int?    _pendingMachineId;

    public MachineViewPage()
    {
        InitializeComponent();
        DataContextChanged += MachineViewPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int? machineId = null;

        // Handle both int and MachineViewNavigationParameter
        if(e.Parameter is int intId)
            machineId = intId;
        else if(e.Parameter is MachineViewNavigationParameter navParam)
        {
            machineId         = navParam.MachineId;
            _navigationSource = navParam.NavigationSource;
        }

        if(machineId.HasValue)
        {
            _pendingMachineId = machineId;

            if(DataContext is MachineViewViewModel viewModel)
            {
                viewModel.SetNavigationSource(_navigationSource);
                _ = viewModel.LoadMachineAsync(machineId.Value);
            }
        }
    }

    private void MachineViewPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is MachineViewViewModel viewModel && _pendingMachineId.HasValue)
        {
            viewModel.SetNavigationSource(_navigationSource);
            _ = viewModel.LoadMachineAsync(_pendingMachineId.Value);
        }
    }
}