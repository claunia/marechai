/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

namespace Marechai.Server.Auth;

/// <summary>
///     JWT audience values issued by <see cref="Marechai.Server.Services.TokenService" />. Used to scope tokens so a
///     short-lived 2FA-pending token cannot be replayed against ordinary API endpoints.
/// </summary>
public static class Audiences
{
    /// <summary>
    ///     Suffix appended to the configured <c>Jwt:Audience</c> value to produce the audience for short-lived
    ///     "two-factor pending" tokens. Tokens with this audience are accepted only by the
    ///     <c>POST /auth/login/two-factor</c>, <c>POST /auth/login/recovery</c> and
    ///     <c>POST /auth/login/two-factor/email/send</c> endpoints; the global JwtBearer configuration rejects them
    ///     for every other route because <c>TokenValidationParameters.ValidAudience</c> is locked to the API value.
    /// </summary>
    public const string Pending2FaSuffix = ":2fa-pending";

    /// <summary>
    ///     Builds the pending-2FA audience by appending <see cref="Pending2FaSuffix" /> to the configured base
    ///     audience. Both the issuer and the validator must agree on this composition.
    /// </summary>
    public static string Pending2Fa(string baseAudience) => baseAudience + Pending2FaSuffix;
}
