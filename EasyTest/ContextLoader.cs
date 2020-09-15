namespace EasyTest
{
    public static class ContextLoader
    {
        public static IContextLoader<TContext> For<TContext>() where TContext : TestContext, new()
        {
            return new ContextLoaderImpl<TContext>();
        }
    }
}