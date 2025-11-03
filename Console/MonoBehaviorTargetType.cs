

namespace DevTools
{
	namespace Console
    {
        /// <summary>
		/// Defines how a command targets MonoBehaviour instances
		/// </summary>
		public enum MonoBehaviorTargetType
		{
			/// <summary>First active and enabled instance found</summary>
			Single,
			
			/// <summary>All active and enabled instances</summary>
			All,
			
			/// <summary>First instance found, including inactive</summary>
			SingleInactive,
			
			/// <summary>All instances, including inactive</summary>
			AllInactive,
			
			/// <summary>Singleton instance (auto-created if needed)</summary>
			Singleton,
			
			/// <summary>Instance must be provided as first argument</summary>
			Argument
		}
    }
}