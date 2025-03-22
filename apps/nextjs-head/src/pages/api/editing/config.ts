import { EditingConfigMiddleware } from '@sitecore-jss/sitecore-jss-nextjs/editing';
import { components } from 'src/componentBuilder';
//import metadata from 'cfg/metadata.json';

/**
 * This Next.js API route is used by Sitecore editors (Pages) in XM Cloud
 * to determine feature compatibility and configuration.
 */

const fakePackagesList = {
  "packages": {
    "@sitecore-cloudsdk/core": "0.5.0",
    "@sitecore-cloudsdk/events": "0.5.0",
    "@sitecore-cloudsdk/personalize": "0.5.0",
    "@sitecore-cloudsdk/utils": "0.5.0",
    //"@sitecore-feaas/clientside": "0.5.19",
    "@sitecore-jss/sitecore-jss": "22.5.2",
    "@sitecore-jss/sitecore-jss-cli": "22.5.2",
    "@sitecore-jss/sitecore-jss-dev-tools": "22.5.2",
    "@sitecore-jss/sitecore-jss-nextjs": "22.5.2",
    "@sitecore-jss/sitecore-jss-react": "22.5.2",
    //"@sitecore/byoc": "0.2.16",
    //"@sitecore/components": "2.0.1"
  }
}
const metadata = fakePackagesList;

const handler = new EditingConfigMiddleware({
  components,
  metadata,
}).getHandler();

export default handler;
