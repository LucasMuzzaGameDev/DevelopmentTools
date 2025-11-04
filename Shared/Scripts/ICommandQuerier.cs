using System.Collections.Generic;

namespace DevTools.Console
{
    public interface ICommandQuerier
    {
        string Name { get; }
        CommandDiscoveryResult DiscoverCommands();
    }

    public struct CommandDiscoveryResult
    {
        public bool Success;
        public IEnumerable<DiscoveredCommand> Commands;
        public string ErrorMessage;
        
        public static CommandDiscoveryResult SuccessResult(IEnumerable<DiscoveredCommand> commands) 
            => new CommandDiscoveryResult { Success = true, Commands = commands };
            
        public static CommandDiscoveryResult FailureResult(string error) 
            => new CommandDiscoveryResult { Success = false, ErrorMessage = error };
    }
}