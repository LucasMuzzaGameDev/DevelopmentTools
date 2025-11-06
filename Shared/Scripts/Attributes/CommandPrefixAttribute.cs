using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CommandPrefixAttribute : Attribute
{
	public readonly string Prefix;
	public CommandPrefixAttribute(string prefix) => Prefix = prefix;
}