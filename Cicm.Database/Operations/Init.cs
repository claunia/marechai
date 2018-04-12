/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Init.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operation to initialize a new database.
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
using System.Data;
using Cicm.Database.Schemas.Sql;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool InitializeNewDatabase()
        {
            Console.WriteLine("Creating new database version {0}", DB_VERSION);

            try
            {
                IDbCommand dbCmd = dbCon.CreateCommand();

                Console.WriteLine("Creating table `admins`");
                dbCmd.CommandText = V2.Admins;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `browser_test`");
                dbCmd.CommandText = V2.BrowserTests;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `Companias`");
                dbCmd.CommandText = V2.Companies;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `computers`");
                dbCmd.CommandText = V2.Computers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `consoles`");
                dbCmd.CommandText = V2.Consoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `console_company`");
                dbCmd.CommandText = V2.ConsoleCompanies;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `cpu`");
                dbCmd.CommandText = V2.Cpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `admins`");
                dbCmd.CommandText = V2.Admins;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `DSPs`");
                dbCmd.CommandText = V2.Dsps;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `forbidden`");
                dbCmd.CommandText = V2.Forbidden;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `Formatos_de_disco`");
                dbCmd.CommandText = V2.DiskFormats;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus`");
                dbCmd.CommandText = V2.Gpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `log`");
                dbCmd.CommandText = V2.Logs;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `money_donation`");
                dbCmd.CommandText = V2.MoneyDonations;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `mpus`");
                dbCmd.CommandText = V2.Mpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `news`");
                dbCmd.CommandText = V2.News;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `own_computer`");
                dbCmd.CommandText = V2.OwnComputers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `own_consoles`");
                dbCmd.CommandText = V2.OwnConsoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `procesadores_principales`");
                dbCmd.CommandText = V2.ProcesadoresPrincipales;
                dbCmd.ExecuteNonQuery();

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error creating database.");
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}