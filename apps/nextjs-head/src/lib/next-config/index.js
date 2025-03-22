const componentPropsPlugin = require('./plugins/component-props');
const corsHeaderPlugin = require('./plugins/cors-header');
//const feaasPlugin = require('./plugins/feaas');
const graphqlPlugin = require('./plugins/graphql');
//const robotsPlugin = require('./plugins/robots');
//const sassPlugin = require('./plugins/sass');
//const sitemapPlugin = require('./plugins/sitemap');

module.exports = [
  componentPropsPlugin,
  corsHeaderPlugin,
  //feaasPlugin,
  graphqlPlugin,
  //robotsPlugin,
  //sassPlugin,
  //sitemapPlugin,
];    