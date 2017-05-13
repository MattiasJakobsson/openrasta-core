﻿using System.Threading.Tasks;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class YieldBeforeMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly string _yieldName;

    public YieldBeforeMiddleware(string yieldName)
    {
      _yieldName = yieldName;
    }

    public async Task Invoke(ICommunicationContext env)
    {
      var yielder = env.Yielder(_yieldName);
      var resumer = env.Resumer(_yieldName);

      yielder.SetResult(true);
      await resumer.Task;
      await Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }
}