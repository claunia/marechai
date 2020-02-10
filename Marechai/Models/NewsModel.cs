using System;

namespace Marechai.Models
{
    public class NewsModel
    {
        public readonly int AffectedId;
        public readonly string Text;
        public readonly DateTime Timestamp;
        public readonly string Controller;
        public readonly string Action;
        public readonly string ItemName;

        public NewsModel(int affectedId, string text, DateTime timestamp, string controller, string action, string itemName)
        {
            AffectedId = affectedId;
            Text = text;
            Timestamp = timestamp;
            Controller = controller;
            Action = action;
            ItemName = itemName;
        }
    }
}