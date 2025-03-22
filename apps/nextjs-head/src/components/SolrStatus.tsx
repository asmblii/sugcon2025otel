import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/solrcorestatus"

async function fetchApi() {
    const response = await fetch(url);
    const result = await response.json();
    return { cores: result };
}

export const getStaticProps: GetStaticComponentProps = async () => {
    return await fetchApi();
};

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchApi();
};


type ComponentProps = {
    cores: Array<{ 
        name: string;
        index: { [key: string]: string };
    }>;
};

export const Default = (props: ComponentProps): JSX.Element => {
    return (
        <div className="component">
            {props.cores.map(obj => (
                <div key={obj.name}>
                    <h2>SolR {obj.name} Status</h2>
                    <dl>
                        <dt>Size:</dt>
                        <dd>{obj.index?.size}</dd>
                    </dl>
                </div>
            ))}
        </div>
    )
}
