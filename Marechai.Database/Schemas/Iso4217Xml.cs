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