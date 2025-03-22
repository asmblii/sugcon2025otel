import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/err"

async function fetchApi() {
    const response = await fetch(url);
    const result = await response.json();
    return { response: result };
}

export const getStaticProps: GetStaticComponentProps = async () => {
    return await fetchApi();
};

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchApi();
};


export const Default = (): JSX.Element => {
    return (
        <div className="component">
            <p>All good here</p>
        </div>
    )
}
