using HotChocolate;

namespace Illusion.Service.Storage.GraphQL.Helpers
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            return error
                .WithMessage(error.Exception.Message)
                .WithCode(ErrorInfoProvider.GetErrorCode(error.Exception.GetType()));
        }
    }
}