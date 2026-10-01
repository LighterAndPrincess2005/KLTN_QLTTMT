// Reuse the original website photos; course details still come from the API.
const photo = (id: string) => `https://images.unsplash.com/${id}?auto=format&fit=crop&w=900&q=85`;

const images: Record<string, string> = {
  JUNIOR: photo('photo-1596464716127-f2a82984de30'),
  FOUNDATION: photo('photo-1541961017774-22349e4a1262'),
  PRE_BASIC: photo('photo-1513364776144-60967b0f800f'),
  BASIC: photo('photo-1579783902614-a3fb3927b6a5'),
  INTERMEDIATE: photo('photo-1549490349-8643362247b5'),
  ACRYLIC: photo('photo-1577083288073-40892c0860a4'),
  CLAY: photo('photo-1565193566173-7a0ee3dbe261'),
};

export const courseImage = (level: string): string =>
  images[level.trim().toUpperCase().replace(/[-\s]+/g, '_')] || images.PRE_BASIC;
