import {
  LayoutService,
  GraphQLLayoutService,
  LayoutServiceData,
} from '@sitecore-jss/sitecore-jss-nextjs';
import clientFactory from 'lib/graphql-client-factory';
import { Exception, trace } from '@opentelemetry/api';

const tracer = trace.getTracer('sitecore-layout-server');

/**
 * Factory responsible for creating a LayoutService instance
 */
export class LayoutServiceFactory {
  /**
   * @param {string} siteName site name
   * @returns {LayoutService} service instance
   */
  create(siteName: string): LayoutService {
    const resultServiceInstance = new GraphQLLayoutService({
      siteName,
      clientFactory,
      /*
        GraphQL endpoint may reach its rate limit with the amount of requests it receives and throw a rate limit error.
        GraphQL Dictionary and Layout Services can handle rate limit errors from server and attempt a retry on requests.
        For this, specify the number of 'retries' the GraphQL client will attempt.
        By default it is set to 3. You can disable it by configuring it to 0 for this service.

        Additionally, you have the flexibility to customize the retry strategy by passing a 'retryStrategy'.
        By default it uses the `DefaultRetryStrategy` with exponential back-off factor of 2 and handles error codes 429,
        502, 503, 504, 520, 521, 522, 523, 524, 'ECONNRESET', 'ETIMEDOUT' and 'EPROTO' . You can use this class or your own implementation of `RetryStrategy`.
      */
      retries: (process.env.GRAPH_QL_SERVICE_RETRIES &&
        parseInt(process.env.GRAPH_QL_SERVICE_RETRIES, 10)) as number,
    });

    /**********************************************
     * 
     * PATCH GraphQLLayoutService to create span during request
     * 
     */

    const originalMethod = resultServiceInstance.fetchLayoutData;
    resultServiceInstance.fetchLayoutData = async function (itemPath: string, language?: string): Promise<LayoutServiceData> {
      const layoutResult = await tracer.startActiveSpan('fetchLayoutData',
        {
          attributes: {
            itemPath,
            language
          }
        },
        async span => {
          try {
            return await originalMethod.apply(resultServiceInstance, [itemPath, language]);
          } catch (err) {
            span.recordException(err as Exception);
            return null;
          } finally {
            span.end();
          }
        });

      // Ok it could actually be null, if there was an error.... let's ignore that...
      return layoutResult as LayoutServiceData;
    };

    return resultServiceInstance;
  }
}

/** LayoutServiceFactory singleton */
export const layoutServiceFactory = new LayoutServiceFactory();
