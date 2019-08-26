HTML TO AMP Converter C#
Dependencys  HtmlAgilityPack
This contains some EPiServer gunk that can be ignored.


namespace Something.Helpers
{
    public class AmpFilterHelper
    {
        public static UrlResolver GetUrlResolver => ServiceLocator.Current.GetInstance<UrlResolver>();
        /// <summary>
        /// Try to Filter TinyMCE html to AMP...
        /// </summary>
        /// <param name="xhtmlString"></param>
        /// <returns>Filtered xhtmlString</returns>
        public string AmpifyEditorsXhtmlStringOutput(string xhtmlString)
        {
            var sourceDocument = new HtmlDocument();
            sourceDocument.LoadHtml(xhtmlString);

            ConvertIframe(sourceDocument);
            RemoveTagAttribute(sourceDocument, "a", "target");
            RemoveTag(sourceDocument, "picture");
            RemoveTag(sourceDocument, "object", false);

            var filteredHtml = ConvertImageTag(sourceDocument, "img");

            return filteredHtml.DocumentNode.WriteContentTo();
        }

        public HtmlDocument ConvertImageTag(HtmlDocument webdoc, string tagToConvert)
        {
            if (webdoc?.DocumentNode != null)
                foreach (HtmlNode node in webdoc.DocumentNode.Descendants(tagToConvert).Where(e => {
                    string src = e.GetAttributeValue("src", null) ?? "";
                    return !string.IsNullOrEmpty(src);
                }))
                {
                    string src = node.GetAttributeValue("src", null);
                    var realPath = GetUrlResolver.GetUrl(src);

                    
                    if (src != null && realPath !=null)
                    {
                        var cref = UrlResolver.Current.Route(new UrlBuilder(realPath));
                        BlobImageProperties imageProperties = null;
                        if (cref != null && cref.ContentLink !=null)
                        {
                             imageProperties = new BlobImageProperties(427, 640); //ExtractImageSizeInformation(cref.ContentLink);
                        }
                       
                        node.Name = "amp-img";
                        node.SetAttributeValue("src", realPath);
                        node.SetAttributeValue("width", imageProperties?.Width == 0 ? "640" : imageProperties?.Width.ToString());
                        node.SetAttributeValue("height", imageProperties?.Height == 0 ? "336" : imageProperties?.Height.ToString());
                        node.SetAttributeValue("layout", "responsive");
                    }

                }

            return webdoc;
        }
        public string ConvertImageTag(ContentReference cRef, string imageSizeSelect = "")
        {
            if (cRef != null)
            {
                string realPath = GetUrlResolver.GetUrl(cRef);
                var imageProperties = new BlobImageProperties(427, 640); // ExtractImageSizeInformation(cRef);
                if (imageProperties == null) { return string.Empty; }

                return $"<amp-img src='{realPath}/{imageSizeSelect}' layout='responsive' alt='Welcome' height='{imageProperties.Height}' width='{imageProperties.Width}'></amp-img>";
            }

            return string.Empty;
        }

        private void ConvertIframe(HtmlDocument htmlText)
        {
            var iframeNodes = htmlText.DocumentNode.SelectNodes("//iframe[@src]");
            if (iframeNodes == null) { return; }

            foreach (var node in iframeNodes)
            {
                var clonedNode = node.CloneNode("amp-youtube");
                clonedNode.Name = "amp-youtube";
                var srcNode = node.GetAttributeValue("src", null);
                var extractedEmbedCode = string.Empty;
                int position = srcNode.LastIndexOf('/');
                if (position > -1)
                {
                    extractedEmbedCode = srcNode.Substring(position + 1);
                }

                clonedNode.SetAttributeValue("layout", "responsive");
                clonedNode.SetAttributeValue("data-videoid", extractedEmbedCode.Replace("?rel=0", string.Empty));
                clonedNode.SetAttributeValue("width", "640");
                clonedNode.SetAttributeValue("height", node.GetAttributeValue("height", null));
                clonedNode.Attributes["src"]?.Remove();

                node.ParentNode.ReplaceChild(clonedNode, node);

            }

        }

        private void RemoveTagAttribute(HtmlDocument htmlText, string tagToLockFor,
            string attributeToRemove)
        {
            var iframeNodes = htmlText.DocumentNode.SelectNodes($"//{tagToLockFor}");
            if (iframeNodes == null) { return; }
            foreach (var node in iframeNodes)
            {
                node.Attributes.Remove(attributeToRemove);
            }

        }
        
        private void RemoveTag(HtmlDocument htmlText, string tagToLockFor, bool keepGrandChildren = true)
        {
            var node = htmlText.DocumentNode.SelectSingleNode($"//{tagToLockFor}");
            node?.ParentNode.RemoveChild(node, keepGrandChildren);
        }

        private BlobImageProperties ExtractImageSizeInformation(ContentReference imageCRef)
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            MediaData mediaDataImage = repo.Get<MediaData>(imageCRef);

            if (mediaDataImage == null) { return null; }

            using (var image = Image.FromStream(mediaDataImage.BinaryData.OpenRead()))
            {
                var imageHeight = image.Height;
                var imageWidth = image.Width;

                return new BlobImageProperties(imageHeight, imageWidth);
            }
            
        }

        private class BlobImageProperties
        {
            public BlobImageProperties(int height, int width)
            {
                Height = height;
                Width = width;
            }

            public  int Height { get; set; }
            public  int Width { get; set; }
        }

    }
}


