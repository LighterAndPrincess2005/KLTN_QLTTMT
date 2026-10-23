// Reuse the original website photos; course details still come from the API.
const photo = (id: string) => `/bui/${id}`;

const images: Record<string, string> = {
  JUNIOR: photo('children.webp'),
  FOUNDATION: photo('poster.jpg'),
  PRE_BASIC: photo('watercolor.jpg'),
  BASIC: photo('sketch.jpg'),
  INTERMEDIATE: photo('oil.jpg'),
  ACRYLIC: photo('acrylic.jpg'),
  CLAY: 'https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?auto=format&fit=crop&w=900&q=85',
};

export const courseImage = (level: string): string =>
  images[level.trim().toUpperCase().replace(/[-\s]+/g, '_')] || images.PRE_BASIC;
