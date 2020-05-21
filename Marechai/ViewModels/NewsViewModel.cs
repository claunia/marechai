/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : NewsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     News view model
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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;

namespace Marechai.ViewModels
{
    public sealed class NewsViewModel
    {
        public NewsViewModel(int affectedId, string text, DateTime timestamp, string controller, string itemName)
        {
            AffectedId = affectedId;
            Text       = text;
            Timestamp  = timestamp;
            Controller = controller;
            ItemName   = itemName;
        }

        public int      AffectedId { get; }
        public string   Controller { get; }
        public string   ItemName   { get; }
        public string   Text       { get; }
        public DateTime Timestamp  { get; }
    }
}