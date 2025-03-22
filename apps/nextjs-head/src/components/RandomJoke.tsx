import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";
import styles from  './RandomJoke.module.css';

const url = process.env.API_URL + "/randomdadjoke"

async function fetchJoke() {
    console.log('Requesting ' + url);
    try {
        const response = await fetch(url);
        const result = await response.json();
        const { joke }: { joke: string } = result;
        return { joke }
    }
    catch {
        return { joke: "Do you know what's worse than a bad joke? A missing joke" };
    }
}

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchJoke();
};

export const getStaticProps: GetStaticComponentProps = async () => {
    return await fetchJoke();
};

export const Default = (props: any) => {
    return (
        <div className={'component ' + styles.quote}>
            <blockquote>
                {props.joke}
            </blockquote>
        </div>
    )
}