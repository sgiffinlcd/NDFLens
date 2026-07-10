using CodeFactory.NDF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NDFLens.Data.Sql
{
    /// <summary>
    /// Provides dependency injection management for this library.
    /// </summary>
    public class LibraryLoader : DependencyInjectionLoader
    {
        /// <summary>
        /// Loads child libraries that are subscribed to by this library.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadLibraries(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Add child libraries to load if necessary.
        }

        /// <summary>
        /// Loads dependency injections that are set up and configured manually.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Manual register singleton and other services that cannot be loaded through automation.
        }


        /// <summary>
        /// Loads dependency injections that are set up and configured manually.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Do not modify this method. It is used to load dependency injections that are set up and configured automatically.
        }
    }
}
