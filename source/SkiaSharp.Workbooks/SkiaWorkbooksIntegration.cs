using Xamarin.Interactive;

[assembly: AgentIntegration (typeof (SkiaSharp.Workbooks.SkiaWorkbooksIntegration))]

namespace SkiaSharp.Workbooks
{
	sealed class SkiaWorkbooksIntegration : IAgentIntegration
	{
		public void IntegrateWith (IAgent agent)
			=> agent.RepresentationManager.AddProvider (new SkiaSharpRepresentationProvider ());
	}
}
