using System;
using System.Collections.Generic;
using System.Reflection;

namespace DevTools.Console
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class SuggestionsAttribute : Attribute
	{
		public string[] StaticSuggestions { get; }
		public string MethodName { get; }
		public Type ProviderType { get; }

		public SuggestionsAttribute(params string[] staticSuggestions)
		{
			StaticSuggestions = staticSuggestions;
		}

		public SuggestionsAttribute(string methodName)
		{
			MethodName = methodName;
		}

		public SuggestionsAttribute(Type providerType)
		{
			ProviderType = providerType;
		}

		public IEnumerable<string> GetSuggestions(object contextInstance, ParameterInfo parameter)
		{
			// 1️⃣ Static
			if (StaticSuggestions != null && StaticSuggestions.Length > 0)
				return StaticSuggestions;

			// 2️⃣ Method-based
			if (!string.IsNullOrEmpty(MethodName))
			{
				var type = parameter.Member.DeclaringType;
				var method = type?.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null && typeof(IEnumerable<string>).IsAssignableFrom(method.ReturnType))
				{
					return (IEnumerable<string>)method.Invoke(contextInstance, null);
				}
			}

			// 3️⃣ Provider-based
			if (ProviderType != null && typeof(ISuggestionProvider).IsAssignableFrom(ProviderType))
			{
				var provider = (ISuggestionProvider)Activator.CreateInstance(ProviderType);
				return provider.GetSuggestions(parameter, string.Empty);
			}

			return Array.Empty<string>();
		}
	}

	public interface ISuggestionProvider
	{
		IEnumerable<string> GetSuggestions(ParameterInfo parameter, string input);
	}
}

