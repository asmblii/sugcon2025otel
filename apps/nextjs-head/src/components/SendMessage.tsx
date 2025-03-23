export const Default = () => {
    const handleClick = async () => {
        try {
            const response = await fetch('/api/send-message', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ requestIdentifier: "hello-from-nextjs-head" }),
            });

            const result = await response.json();
            console.log("Result from API:", result);
        } catch (error) {
            console.error("Error calling server-side API:", error);
        }
    };

    return (
        <div className="component">
            <code>flow: nextjs-head -&gt; dotnet-api -&gt; azure service bus -&gt; dotnet message listener</code>
            <div>
                <button onClick={handleClick}>Send Message!</button>
            </div>
        </div>
    );
};