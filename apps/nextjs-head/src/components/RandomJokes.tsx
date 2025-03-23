import type { GetServerSideComponentProps, GetStaticComponentProps } from "@sitecore-jss/sitecore-jss-nextjs";

const url = process.env.API_URL + "/randomdadjokes";

async function fetchJokes() {
    console.log('Requesting ' + url);

    try {
        const response = await fetch(url);
        const result = await response.json();

        if (Array.isArray(result) && result.length > 0) {
            return { jokes: result.map((jokeObj: any) => jokeObj.joke) };
        } else {
            return { jokes: ["No jokes available at the moment."] };
        }
    } catch {
        return { jokes: ["Do you know what's worse than a bad joke? A missing joke"] };
    }
}

export const getServerSideProps: GetServerSideComponentProps = async () => {
    return await fetchJokes();
};

export const getStaticProps: GetStaticComponentProps = async () => {
    return await fetchJokes();
};

export const Default = (props: any) => {
    return (
        <div className="component">
            <code>flow: nextjs-head -&gt; dotnet-api -&gt; external api x 3</code>
            <h3>Random dad jokes</h3>
            {props.jokes && props.jokes.length > 0 ? (
                props.jokes.map((joke: string, index: number) => (
                    <blockquote key={index}>
                        "{joke}"
                    </blockquote>
                ))
            ) : (
                <blockquote>No jokes to display.</blockquote>
            )}
        </div>
    );
};