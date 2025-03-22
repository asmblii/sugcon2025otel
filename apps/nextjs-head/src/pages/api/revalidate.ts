import type { NextApiRequest, NextApiResponse } from 'next';


export default async function (req: NextApiRequest, res: NextApiResponse): Promise<void> {
    if (req.query.secret !== process.env.REVALIDATION_SECRET) {
        return res.status(401).json({ message: 'Invalid token' })
    }
    const pathToRevalidate = req.query.path as string;
    try {
        await res.revalidate(pathToRevalidate)
        return res.json({ revalidated: true })
    } catch (err) {
        return res.status(500).send('Error revalidating')
    }
}