using System;
using UnityEngine;

namespace DevTools.Console
{
	public class ParameterRegistry 
	{
		public static ParameterRegistry Instance { get; } = new ParameterRegistry();
		public void DiscoverParameters()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type != typeof(IParameterParser)) continue;
					
					if (type == typeof(IParameterParser))
					{
						
					}
				}
			}
		}	
	}
}