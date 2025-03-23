import type { NextApiRequest, NextApiResponse } from 'next';

const url = process.env.API_URL + "/busrequest";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
    if (req.method === 'POST') {
        const { requestIdentifier } = req.body;

        try {
            const response = await fetch(`${url}/${requestIdentifier}`, {
                method: 'GET', // Assuming the external API uses GET
            });
            const result = await response.json();

            res.status(200).json({ response: result });
        } catch (error) {
            console.error("Error calling busrequest API:", error);
            res.status(500).json({ message: "Failed to call busrequest API" });
        }
    } else {
        res.status(405).json({ message: "Method not allowed" });
    }
}