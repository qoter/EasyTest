namespace EasyTest
{
    public static class ContentLoader
    {
        public static IContentLoader<TContent> For<TContent>() where TContent : TestContent, new()
        {
            return new ContentLoaderImpl<TContent>();
        }
    }
}