/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Program.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Main class.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using DiscImageChef.Interop;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Version = DiscImageChef.Interop.Version;

namespace cicm_web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();

            Console.Write(
                          "\u001b[32m                             .                ,,\n"                                                                                                                                                                         +
                          "\u001b[32m                          ;,.                  '0d.\n"                                                                                                                                                                      +
                          "\u001b[32m                        oc                       oWd                      \u001b[31m"                                                                                                                                       +
                          @"________/\\\\\\\\\___/\\\\\\\\\\\_________/\\\\\\\\\___/\\\\____________/\\\\_"                                                                                                                                                      +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                      ;X.                         'WN'                    \u001b[31m"                                                                                                                                       +
                          @" _____/\\\////////___\/////\\\///_______/\\\////////___\/\\\\\\________/\\\\\\_"                                                                                                                                                     +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                     oMo                           cMM:                   \u001b[31m"                                                                                                                                       +
                          @"  ___/\\\/________________\/\\\________/\\\/____________\/\\\//\\\____/\\\//\\\_"                                                                                                                                                    +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                    ;MM.                           .MMM;                  \u001b[31m"                                                                                                                                       +
                          @"   __/\\\__________________\/\\\_______/\\\______________\/\\\\///\\\/\\\/_\/\\\_"                                                                                                                                                   +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                    NMM                             WMMW                  \u001b[31m"                                                                                                                                       +
                          @"    _\/\\\__________________\/\\\______\/\\\______________\/\\\__\///\\\/___\/\\\_"                                                                                                                                                  +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                   'MMM                             MMMM;                 \u001b[31m"                                                                                                                                       +
                          @"     _\//\\\_________________\/\\\______\//\\\_____________\/\\\____\///_____\/\\\_"                                                                                                                                                 +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                   ,MMM:                           dMMMM:                 \u001b[31m"                                                                                                                                       +
                          @"      __\///\\\_______________\/\\\_______\///\\\___________\/\\\_____________\/\\\_"                                                                                                                                                +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                   .MMMW.                         :MMMMM.                 \u001b[31m"                                                                                                                                       +
                          @"       ____\////\\\\\\\\\___/\\\\\\\\\\\_____\////\\\\\\\\\__\/\\\_____________\/\\\_"                                                                                                                                               +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                    XMMMW:    .:xKNMMMMMMN0d,    lMMMMMd                  \u001b[31m"                                                                                                                                       +
                          @"        _______\/////////___\///////////_________\/////////___\///______________\///__"                                                                                                                                              +
                          "\n\u001b[0m"                                                                                                                                                                                                                          +
                          "\u001b[32m                    :MMMMMK; cWMNkl:;;;:lxKMXc .0MMMMMO\u001b[0m\n"                                                                                                                                                         +
                          "\u001b[32m                   ..KMMMMMMNo,.             ,OMMMMMMW:,.                 \u001b[37;1m          Canary Islands Computer Museum Website\u001b[0m\n"                                                                          +
                          "\u001b[32m            .;d0NMMMMMMMMMMMMMMW0d:'    .;lOWMMMMMMMMMMMMMXkl.            \u001b[37;1m          Version \u001b[0m\u001b[33m{0}\u001b[37;1m-\u001b[0m\u001b[31m{1}\u001b[0m\n"                                               +
                          "\u001b[32m          :KMMMMMMMMMMMMMMMMMMMMMMMMc  WMMMMMMMMMMMMMMMMMMMMMMWk'\u001b[0m\n"                                                                                                                                               +
                          "\u001b[32m        ;NMMMMWX0kkkkO0XMMMMMMMMMMM0'  dNMMMMMMMMMMW0xl:;,;:oOWMMX;       \u001b[37;1m          Running under \u001b[35;1m{2}\u001b[37;1m, \u001b[35m{3}-bit\u001b[37;1m in \u001b[35m{4}-bit\u001b[37;1m mode.\u001b[0m\n" +
                          "\u001b[32m       xMMWk:.           .c0MMMMMW'      OMMMMMM0c'..          .oNMO      \u001b[37;1m          Using \u001b[33;1m{5}\u001b[37;1m version \u001b[31;1m{6}\u001b[0m\n"                                                       +
                          "\u001b[32m      OMNc            .MNc   oWMMk       'WMMNl. .MMK             ;KX.\u001b[0m\n"                                                                                                                                          +
                          "\u001b[32m     xMO               WMN     ;  .,    ,  ':    ,MMx               lK\u001b[0m\n"                                                                                                                                          +
                          "\u001b[32m    ,Md                cMMl     .XMMMWWMMMO      XMW.                :\u001b[0m\n"                                                                                                                                          +
                          "\u001b[32m    Ok                  xMMl     XMMMMMMMMc     0MW,\u001b[0m\n"                                                                                                                                                            +
                          "\u001b[32m    0                    oMM0'   lMMMMMMMM.   :NMN'\u001b[0m\n"                                                                                                                                                             +
                          "\u001b[32m    .                     .0MMKl ;MMMMMMMM  oNMWd\u001b[0m\n"                                                                                                                                                               +
                          "\u001b[32m                            .dNW cMMMMMMMM, XKl\u001b[0m\n"                                                                                                                                                                 +
                          "\u001b[32m                                 0MMMMMMMMK\u001b[0m\n"                                                                                                                                                                     +
                          "\u001b[32m                                ;MMMMMMMMMMO                              \u001b[37;1m          Proudly presented to you by:\u001b[0m\n"                                                                                    +
                          "\u001b[32m                               'WMMMMKxMMMMM0                             \u001b[34;1m          Natalia Portillo\u001b[0m\n"                                                                                                +
                          "\u001b[32m                              oMMMMNc  :WMMMMN:\u001b[0m\n"                                                                                                                                                                 +
                          "\u001b[32m                           .dWMMM0;      dWMMMMXl.                        \u001b[37;1m          Thanks to all contributors, collaborators, translators, donators and friends.\u001b[0m\n"                                   +
                          "\u001b[32m               .......,cd0WMMNk:           c0MMMMMWKkolc:clodc'\u001b[0m\n"                                                                                                                                                 +
                          "\u001b[32m                 .';loddol:'.                 ':oxkkOkkxoc,.\u001b[0m\n"                                                                                                                                                    +
                          "\u001b[0m\n", Version.GetVersion(),
                          #if DEBUG
                          "DEBUG"
                          #else
                          "RELEASE"
                                                                              #endif
                        , DetectOS.GetPlatformName(DetectOS.GetRealPlatformID()),
                          Environment.Is64BitOperatingSystem ? 64 : 32, Environment.Is64BitProcess ? 64 : 32,
                          DetectOS.IsMono ? "Mono" : ".NET Core",
                          DetectOS.IsMono ? Version.GetMonoVersion() : Version.GetNetCoreVersion());

            Console.WriteLine("\u001b[31;1mStarting web server...\u001b[0m");

            BuildWebHost(args).Run();
        }

        static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().UseUrls("http://*:5000").Build();
        }
    }
}