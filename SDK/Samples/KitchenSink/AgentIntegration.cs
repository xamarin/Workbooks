using System;
using System.Collections;
using System.Collections.Generic;

using Xamarin.Interactive;
using Xamarin.Interactive.Representations;

[assembly: AgentIntegration (typeof (KitchenSinkIntegration.AgentIntegration))]

namespace KitchenSinkIntegration
{
	class AgentIntegration : IAgentIntegration
	{
		const string TAG = nameof (AgentIntegration);

		public void IntegrateWith (IAgent agent)
		{
			agent.RepresentationManager.AddProvider (new SampleRepresentationProvider ());
		}

		class SampleRepresentationProvider : RepresentationProvider
		{
			public override bool HasSensibleEnumerator (IEnumerable enumerable)
			{
				// for some reason rendering Arrays as enumerables just doesn't
				// make sense in the context of this agent integration!
				if (enumerable is Array)
					return false;

				return base.HasSensibleEnumerator (enumerable);
			}

			public override IEnumerable<object> ProvideRepresentations (object obj)
			{
				// we really like green, so return it for all objects!
				yield return new Color (0, 1, 0, 0.5);
			}
		}
	}
}