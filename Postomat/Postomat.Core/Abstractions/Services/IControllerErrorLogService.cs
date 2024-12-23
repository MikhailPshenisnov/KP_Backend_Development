using Postomat.Core.Contracrs;

namespace Postomat.Core.Abstractions.Services;

public interface IControllerErrorLogService
{
    public Task<BaseResponse<T>> CreateErrorLog<T>(string origin, string title, string message);
}