﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ThreePhasedPipelineAdaptor : IPipeline, IPipelineAsync
  {
    readonly IGenerateCallGraphs _callGrapher;
    Func<ICommunicationContext, Task> _invoker;
    public bool IsInitialized { get; private set; }
    public IList<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; private set; }
    public StartupProperties StartupProperties { get; private set; }

    public ThreePhasedPipelineAdaptor(IDependencyResolver resolver)
    {
      _callGrapher =new CallGraphGeneratorFactory(resolver)
        .GetCallGraphGenerator();
      Contributors = resolver.ResolveAll<IPipelineContributor>()
        .ToList()
        .AsReadOnly();
    }

    public void Initialize()
    {
      Initialize(new StartupProperties());
    }

    public void Initialize(StartupProperties startup)
    {
      if (startup.OpenRasta.Pipeline.Validate)
        Contributors.VerifyKnownStagesRegistered();

      var defaults = new List<IPipelineMiddlewareFactory>();
      if (startup.OpenRasta.Errors.HandleCatastrophicExceptions)
      {
        defaults.Add(new CatastrophicFailureMiddleware());
      }

      var contributorMiddleware = (CallGraph = _callGrapher.GenerateCallGraph(Contributors))
        .ToMiddleware(startup.OpenRasta.Pipeline.ContributorTrailers);
      _invoker =
        defaults.Concat(contributorMiddleware)
        .Compose()
        .Invoke;
      IsInitialized = true;
    }


    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> notification)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    [Obsolete("Don't do it this will deadlock.")]
    public void Run(ICommunicationContext context)
    {
      RunAsync(context).GetAwaiter().GetResult();
    }

    public Task RunAsync(ICommunicationContext env)
    {
      this.CheckPipelineInitialized();

      if (env.PipelineData.PipelineStage == null)
        env.PipelineData.PipelineStage = new PipelineStage(((IPipeline) this).CallGraph);
      return _invoker(env);
    }

    public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }
  }
}