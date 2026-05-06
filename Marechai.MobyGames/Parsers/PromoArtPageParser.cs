using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses the new MobyGames promo art page HTML (post-2024 redesign).
///     Structure: h2 headings for group names, followed by thumbnail grid with image links.
///     Group URLs: /promo/group-{groupId}/
///     Image URLs: /promo/group-{groupId}/image-{imageId}/
/// </summary>
public static partial class PromoArtPageParser
{
    public static List<ParsedPromoArtGroup> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return Parse(doc);
    }

    public static List<ParsedPromoArtGroup> Parse(HtmlDocument doc)
    {
        var groups = new List<ParsedPromoArtGroup>();

        // Find all h2 elements that contain group links with /promo/group-{id}/ pattern
        var headings = doc.DocumentNode.SelectNodes("//h2");

        if(headings is null) return groups;

        foreach(var heading in headings)
        {
            // Skip "Ad Blurbs" heading and other non-promo headings
            var groupLink = heading.SelectSingleNode(".//a[contains(@href, '/promo/group-')]");

            if(groupLink is null) continue;

            string groupName = WebUtility.HtmlDecode(groupLink.InnerText).Trim();
            string groupHref = groupLink.GetAttributeValue("href", "");
            var    groupMatch = GroupIdRegex().Match(groupHref);
            string groupId   = groupMatch.Success ? groupMatch.Groups[1].Value : null;

            var group = new ParsedPromoArtGroup
            {
                GroupName = groupName,
                GroupId   = groupId
            };

            // Find images that follow this heading — look for links with /image-{id}/ pattern
            // in the sibling elements after this h2
            var nextSibling = heading.NextSibling;

            while(nextSibling is not null)
            {
                if(nextSibling.NodeType == HtmlNodeType.Element)
                {
                    // Stop if we hit another h2 (next group)
                    if(nextSibling.Name == "h2") break;

                    // Look for image links within this element
                    var imageLinks = nextSibling.SelectNodes(".//a[contains(@href, '/image-')]");

                    if(imageLinks is not null)
                    {
                        foreach(var link in imageLinks)
                        {
                            string href       = link.GetAttributeValue("href", "");
                            var    imageMatch  = ImageIdRegex().Match(href);

                            if(!imageMatch.Success) continue;

                            string imageId = imageMatch.Groups[1].Value;

                            // Skip duplicates
                            if(group.Images.Any(i => i.ImageId == imageId)) continue;

                            // Caption comes from text content near the image
                            string caption = ExtractCaption(link);

                            // Thumbnail URL from img src
                            var img = link.SelectSingleNode(".//img");
                            string thumbnailUrl = img?.GetAttributeValue("src", null);

                            group.Images.Add(new ParsedPromoArtImage
                            {
                                ImageId       = imageId,
                                DetailPageUrl = href,
                                Caption       = caption,
                                ThumbnailUrl  = thumbnailUrl
                            });
                        }
                    }
                }

                nextSibling = nextSibling.NextSibling;
            }

            if(group.Images.Count > 0)
                groups.Add(group);
        }

        return groups;
    }

    static string ExtractCaption(HtmlNode linkNode)
    {
        // Try to get caption from the parent container's text content
        var parent = linkNode.ParentNode;

        if(parent is null) return null;

        // Look for text nodes after the link within the same container
        var textParts = new List<string>();

        foreach(var child in parent.ChildNodes)
        {
            if(child.NodeType == HtmlNodeType.Text)
            {
                string text = WebUtility.HtmlDecode(child.InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(text))
                    textParts.Add(text);
            }
        }

        return textParts.Count > 0 ? string.Join(" ", textParts) : null;
    }

    [GeneratedRegex(@"group-(\d+)", RegexOptions.Compiled)]
    private static partial Regex GroupIdRegex();

    [GeneratedRegex(@"image-(\d+)", RegexOptions.Compiled)]
    private static partial Regex ImageIdRegex();
}
