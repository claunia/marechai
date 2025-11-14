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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Marechai.Server.Helpers;

/// <summary>Custom route constraint for char parameters</summary>
public class CharRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext    httpContext, IRouter route, string routeKey, RouteValueDictionary values,
                      RouteDirection routeDirection)
    {
        if(!values.TryGetValue(routeKey, out object value)) return false;

        string stringValue = value?.ToString() ?? string.Empty;

        return stringValue.Length == 1 && char.TryParse(stringValue, out _);
    }
}