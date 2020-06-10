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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Xml.Serialization;

namespace Marechai.Database.Schemas
{
    public class Iso4217Xml
    {
        [XmlRoot(ElementName = "ISO_4217")]
        public class Root
        {
            [XmlAttribute(AttributeName = "Pblshd", DataType = "date")]
            public DateTime Published { get; set; }

            [XmlArray("CcyTbl"), XmlArrayItem("CcyNtry", typeof(Currency), IsNullable = true)]
            public Currency[] Current { get; set; }

            [XmlArray("HstrcCcyTbl"), XmlArrayItem("HstrcCcyNtry", typeof(Currency), IsNullable = true)]
            public Currency[] Historical { get; set; }
        }

        public class Currency
        {
            [XmlElement("CtryNm")]
            public string Country { get; set; }

            [XmlElement("Ccy")]
            public string Code { get; set; }

            [XmlElement("CcyNbr")]
            public short Number { get; set; }

            [XmlElement("CcyMnrUnts", IsNullable = true)]
            public string MinorUnits { get; set; }

            [XmlElement("CcyNm")]
            public string Name { get; set; }

            [XmlElement("WthdrwlDt", IsNullable = true)]
            public string Withdrawn { get; set; }
        }
    }
}