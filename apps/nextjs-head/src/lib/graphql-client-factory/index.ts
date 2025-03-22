import config from '@jssconfig';
import { createGraphQLClientFactory } from './create';

// The GraphQLRequestClientFactory serves as the central hub for executing GraphQL requests within the application

// Create a new instance on each import call

const cfg = {
    sitecoreApiKey: config.sitecoreApiKey,
    sitecoreApiHost: config.sitecoreApiHost,
    sitecoreSiteName: config.sitecoreSiteName,
    graphQLEndpointPath: config.graphQLEndpointPath,
    defaultLanguage: config.defaultLanguage,
    graphQLEndpoint: config.graphQLEndpoint,
    layoutServiceConfigurationName: config.layoutServiceConfigurationName,
    publicUrl: config.publicUrl,
}

export default createGraphQLClientFactory(cfg);
