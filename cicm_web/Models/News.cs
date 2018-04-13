using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cicm.Database.Schemas;

namespace cicm_web.Models
{
    public class News
    {
        /// <summary>Affected ID</summary>
        public int AffectedId;
        /// <summary>Date</summary>
        public DateTime Date;
        /// <summary>URL of image</summary>
        public string Image;
        /// <summary>URL of target view, if applicable</summary>
        public string TargetView;
        /// <summary>Text</summary>
        public string Text;

        public static News[] GetAllItems()
        {
            List<Cicm.Database.Schemas.News> dbItems = null;
            bool?                            result  = Program.Database?.Operations.GetNews(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Select(TransformItem) as News[];
        }

        public static News[] GetLastItems(int count = 10)
        {
            List<Cicm.Database.Schemas.News> dbItems = null;
            bool?                            result  = Program.Database?.Operations.GetNews(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Take(count).Select(TransformItem).ToArray();
        }

        static News TransformItem(Cicm.Database.Schemas.News dbItem)
        {
            string imageUrl;
            string text;
            string targetView;

            switch(dbItem.Type)
            {
                case NewsType.NewComputerInDb:
                    text       = "New computer added to the database.";
                    imageUrl   = "assets/photos/computers/";
                    targetView = "computer";
                    break;
                case NewsType.NewConsoleInDb:
                    text       = "New videoconsole added to the database.";
                    imageUrl   = "assets/photos/consoles/";
                    targetView = "console";
                    break;
                case NewsType.NewComputerInCollection:
                    text       = "New computer added to the museum's collection.";
                    imageUrl   = "assets/photos/computers/";
                    targetView = "collection_computer";
                    break;
                case NewsType.NewConsoleInCollection:
                    text       = "New videoconsole added to the museum's collection.";
                    imageUrl   = "assets/photos/consoles/";
                    targetView = "collection_console";
                    break;
                case NewsType.UpdatedComputerInDb:
                    text       = "Updated computer from the database.";
                    imageUrl   = "assets/photos/computers/";
                    targetView = "computer";
                    break;
                case NewsType.UpdatedConsoleInDb:
                    text       = "Updated videoconsole from the database.";
                    imageUrl   = "assets/photos/consoles/";
                    targetView = "console";
                    break;
                case NewsType.UpdatedComputerInCollection:
                    text       = "Updated computer from museum's collection.";
                    imageUrl   = "assets/photos/computers/";
                    targetView = "collection_computer";
                    break;
                case NewsType.UpdatedConsoleInCollection:
                    text       = "Updated videoconsole from museum's collection.";
                    imageUrl   = "assets/photos/consoles/";
                    targetView = "collection_console";
                    break;
                case NewsType.NewMoneyDonation:
                    text       = "New money donation.";
                    imageUrl   = null;
                    targetView = null;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return new News
            {
                AffectedId = dbItem.AffectedId,
                Date       = DateTime.ParseExact(dbItem.Date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                Image      = imageUrl == null ? null : imageUrl + $"{dbItem.AffectedId}",
                Text       = text,
                TargetView = targetView
            };
        }
    }
}