using HotChocolate;
using Illusion.Common.Helpers;

namespace Illusion.Common.GraphQL
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