const config = {
    sitecoreApiKey: process.env.SITECORE_API_KEY || '',
    sitecoreApiHost: process.env.SITECORE_API_HOST || '',
    sitecoreEdgeUrl: process.env.SITECORE_EDGE_URL || '',
    sitecoreEdgeContextId: process.env.SITECORE_EDGE_CONTEXT || '',
    sitecoreSiteName: process.env.SITECORE_SITE_NAME || 'headnextjs',
    publicUrl: process.env.PUBLIC_URL || 'http://localhost:5000/',
    graphQLEndpointPath: process.env.GRAPH_QL_ENDPOINT_PATH || '/sitecore/api/graph/edge',
    defaultLanguage: process.env.DEFAULT_LANGUAGE || 'en',
    locales: ['en'],
    graphQLEndpoint: process.env.GRAPH_QL_ENDPOINT || '',
    layoutServiceConfigurationName: process.env.LAYOUT_SERVICE_CONFIGURATION_NAME || 'sxa-jss',
  };

  if(config.sitecoreApiHost && config.graphQLEndpointPath && !config.graphQLEndpoint) {
    config.graphQLEndpoint = config.sitecoreApiHost + config.graphQLEndpointPath;
  }
  
  module.exports = config;
  