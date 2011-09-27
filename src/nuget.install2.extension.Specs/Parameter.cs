using Moq;

namespace nuget.install2.extension.Specs
{
    public static class Parameter
    {
        public static TValue IsAny<TValue>()
        {
            return It.IsAny<TValue>();
        }
    }
}