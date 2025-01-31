using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Database.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Database.Tarantool.Configuration;

public static class TarantoolServiceProviderExtensions
{
    public static void InitTarantool(this IServiceProvider services,
        ILogger? logger = null)
    {
        var messageRepository = services.GetRequiredService<IMessageRepository>();
        messageRepository.Init();
    }
}
