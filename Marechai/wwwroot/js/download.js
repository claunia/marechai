/******************************************************************************
 // MARECHAI: Master repository of computing history artifacts information
 // --------------------------------------------------------------------------
 //
 // Author(s)      : Natalia Portillo <claunia@claunia.com>
 //
 // --[ License ] ------------------------------------------------------------
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
 // --------------------------------------------------------------------------
 // Copyright © 2003-2026 Natalia Portillo
 *******************************************************************************/

window.downloadFileFromBytes = function (fileName, contentType, base64Content) {
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
