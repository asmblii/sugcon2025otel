import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/sitecore"

async function fetchHi() {
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
    return await fetchHi();
};

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchHi();
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
            <h4>{props.response?.title}</h4>
            <div>{props.response?.content}</div>
        </div>
    )
}
