import { GetServerSideProps } from 'next';
import { sitecorePagePropsFactory } from 'lib/page-props-factory';

// This function gets called when pages are server side rendered on demand
export const getServerSideProps: GetServerSideProps = async (context) => {

  const props = await sitecorePagePropsFactory.create(context);

  return {
    props,
    notFound: props.notFound, // Returns custom 404 page with a status code of 404 when true
  };
};
