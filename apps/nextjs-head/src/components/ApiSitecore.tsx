import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/sitecore"

async function fetchData() {
    try {
        const response = await fetch(url);
        const result = await response.json();
        return { response: result };
    }
    catch {
        return { response: { message: "No sitecore response" } };
    }
}

export const getStaticProps: GetStaticComponentProps = async () => {
    return await fetchData();
};

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchData();
};

type ComponentProps = {
    response: {
        title: string
        content: string
    }
};

export const Default = (props: ComponentProps) => {
    return (
        <div className="component">
            <code>flow: nextjs-head -&gt; dotnet-api -&gt; sitecore graphql -&gt; solr and mssql</code>
            <h3>{props.response?.title}</h3>
            <div dangerouslySetInnerHTML={{ __html: props.response?.content || '' }} />
        </div>
    )
}
