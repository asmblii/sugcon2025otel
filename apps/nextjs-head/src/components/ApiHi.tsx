import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/slowhi"

async function fetchHi() {
    try {
        const response = await fetch(url);
        const result = await response.json();
        return { response: result };
    }
    catch {
        return { response: { message: "No hi" } };
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
        message: string
    }
};

export const Default = (props: ComponentProps) => {
    return (
        <div className="component">
            {props.response?.message}
        </div>
    )
}
