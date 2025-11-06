using System;
using UnityEngine;

namespace DevTools.Console
{
	public interface IParameterParser 
	{
		bool CanParse(Type targetType);
		object Parse(string input, Type targetType, string paramName);
	}
}
